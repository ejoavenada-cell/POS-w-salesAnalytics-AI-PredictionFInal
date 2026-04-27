import pyodbc
import pandas as pd
from prophet import Prophet
from prophet.serialize import model_to_json, model_from_json
import os
import argparse
import sys

# 1. Global Constants
MODEL_PATH = "prophet_model.json"
RESULTS_PATH = "forecast_results.json"

def get_connection():
    """Detects available ODBC drivers and connects to LocalDB."""
    try:
        drivers = [d for d in pyodbc.drivers() if "SQL Server" in d]
        if not drivers:
            print("Error: No SQL Server ODBC drivers found.")
            sys.exit(1)
        
        # Use the most modern driver available
        best_driver = drivers[0]
        for d in drivers:
            if "17" in d or "18" in d:
                best_driver = d
                break
        
        server = "(localdb)\\mssqllocaldb"
        database = "FoodOrderingDB"
        conn_str = f"Driver={{{best_driver}}};Server={server};Database={database};Trusted_Connection=yes;"
        return pyodbc.connect(conn_str)
    except Exception as e:
        print(f"Database connection failure: {e}")
        sys.exit(1)

def fetch_db_data():
    """Fetch transaction data from the local SQL database."""
    print("Connecting to database...")
    try:
        conn = get_connection()
        query = "SELECT CAST(Date AS DATE) as ds, SUM(TotalAmount) as y FROM Transactions GROUP BY CAST(Date AS DATE) ORDER BY ds"
        df = pd.read_sql(query, conn)
        conn.close()
        return df
    except Exception as e:
        print(f"Warning: Could not fetch DB data: {e}")
        return pd.DataFrame()

def load_from_csv(file_path):
    """Load and clean data from a Kaggle or uploaded CSV file."""
    if not file_path or not os.path.exists(file_path):
        return pd.DataFrame()
        
    print(f"Loading external data from: {file_path}")
    try:
        df = pd.read_csv(file_path)
        col_map = {c.lower(): c for c in df.columns}
        
        # Identify necessary columns
        ds_col = next((col_map[k] for k in ['ds', 'date', 'timestamp', 'day'] if k in col_map), None)
        y_col = next((col_map[k] for k in ['total', 'revenue', 'sales', 'gross income', 'y'] if k in col_map), None)

        if not ds_col or not y_col:
            print(f"Warning: Missing required columns in {file_path}")
            return pd.DataFrame()
        
        df_clean = df[[ds_col, y_col]].copy()
        df_clean.columns = ['ds', 'y']
        df_clean['ds'] = pd.to_datetime(df_clean['ds'])
        return df_clean.groupby('ds')['y'].sum().reset_index()
    except Exception as e:
        print(f"Error loading CSV: {e}")
        return pd.DataFrame()

def generate_fallback_data():
    """Generate mock data if no real data is found."""
    print("Cold Start Mode: Generating synthetic growth data...")
    dates = pd.date_range(start='2026-01-01', periods=60)
    y_values = [1000 + (i * 10) + (500 if i % 7 > 4 else 0) for i in range(60)]
    return pd.DataFrame({'ds': dates, 'y': y_values})

def train_and_save(csv_file=None):
    """Hybrid Training Process."""
    db_df = fetch_db_data()
    csv_df = load_from_csv(csv_file)
    
    # Standardize formats
    if not db_df.empty:
        db_df['ds'] = pd.to_datetime(db_df['ds'])
    if not csv_df.empty:
        csv_df['ds'] = pd.to_datetime(csv_df['ds'])
    
    # Merge strategy
    if not db_df.empty and not csv_df.empty:
        print("Hybrid Training: Merging DB + CSV...")
        full_df = pd.concat([db_df, csv_df]).drop_duplicates(subset=['ds']).sort_values('ds')
    elif not db_df.empty:
        full_df = db_df
    elif not csv_df.empty:
        full_df = csv_df
    else:
        full_df = generate_fallback_data()

    print(f"Training on {len(full_df)} samples.")
    model = Prophet(weekly_seasonality=True, seasonality_mode='multiplicative', changepoint_prior_scale=0.1)
    model.fit(full_df)
    
    with open(MODEL_PATH, 'w') as f:
        f.write(model_to_json(model))
    print(f"Model saved: {MODEL_PATH}")

def predict(days=30):
    """Generate and export predictions."""
    if not os.path.exists(MODEL_PATH):
        return
    with open(MODEL_PATH, 'r') as f:
        model = model_from_json(f.read())
    
    future = model.make_future_dataframe(periods=days)
    forecast = model.predict(future)
    
    results = forecast.tail(days)[['ds', 'yhat']].copy()
    results['ds'] = results['ds'].dt.strftime('%Y-%m-%d')
    results.to_json(RESULTS_PATH, orient='records', date_format='iso')
    print(f"Results exported: {RESULTS_PATH}")

def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(description="Prophet AI Suite")
    parser.add_argument("--file", help="Path to training CSV")
    args = parser.parse_args()
    
    train_and_save(csv_file=args.file)
    predict()

if __name__ == "__main__":
    main()
