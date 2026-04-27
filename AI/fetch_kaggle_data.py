import kagglehub
import os
import shutil

def download_dataset(dataset_handle):
    """Download a dataset from Kaggle using kagglehub"""
    print(f"Downloading dataset: {dataset_handle}...")
    try:
        path = kagglehub.dataset_download(dataset_handle)
        print(f"Dataset downloaded to: {path}")
        
        # Look for the CSV file in the downloaded path
        files = os.listdir(path)
        csv_files = [f for f in files if f.endswith('.csv')]
        
        if not csv_files:
            print("No CSV files found in the downloaded dataset.")
            return None
            
        # Copy the first CSV found to the current AI directory for easy access
        source_file = os.path.join(path, csv_files[0])
        dest_file = os.path.join(os.getcwd(), "external_data.csv")
        shutil.copy(source_file, dest_file)
        
        print(f"Successfully prepared external data at: {dest_file}")
        return dest_file
    except Exception as e:
        print(f"Error downloading from Kaggle: {e}")
        return None

if __name__ == "__main__":
    # Use the supermarket sales dataset suggested
    handle = "faresashraf1001/supermarket-sales"
    csv_path = download_dataset(handle)
    
    if csv_path:
        print(f"\nSUCCESS! You can now train the AI using:")
        print(f"py train_prophet.py --file external_data.csv")
