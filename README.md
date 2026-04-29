# 🚀 Tasty Station: AI-Driven POS & Analytics

Tasty Station is a premium, high-performance Food Ordering and Point of Sale (POS) system built with **ASP.NET Core 9** and powered by a **Hybrid Prophet AI Forecasting Engine**. 

Designed for modern restaurant management, it provides real-time sales tracking, kitchen management, and strategic business intelligence.

---

## 🧠 Advanced AI Analytics
The core of Tasty Station is its **Executive AI Dashboard**, which leverages machine learning to predict future revenue and optimize inventory.

### Key AI Features:
- **Prophet Time-Series Forecasting**: Dual-line revenue charts showing "Actual vs. Predicted" sales for the next 30 days.
- **Hybrid Training Suite**: A Python-based training pipeline that merges local SQL transactions with external Big Data (Kaggle) for high-precision modeling.
- **Revenue Velocity Detection**: Real-time detection of "Bullish" or "Correction" market trends with automated strategic recommendations.
- **Category Pulse**: Dynamic share-of-market analysis using interactive doughnut charts and trend velocity metrics.

---

## 🛠️ Technology Stack
- **Backend**: C# / ASP.NET Core 9 (Service-Oriented Architecture)
- **Frontend**: Razor Pages, Vanilla CSS (Premium Glassmorphism Design), Chart.js
- **Database**: SQL Server LocalDB / Entity Framework Core
- **AI/ML Engine**: Python 3.14, Meta Prophet, Pandas, PyODBC

---

## 🚀 Getting Started

### 1. Web Application Setup
```powershell
# Restore dependencies and run the server
dotnet restore
dotnet run
```
The app will be available at `https://localhost:5200` (or the port specified in `launchSettings.json`).

### 2. AI Intelligence Setup
To activate the forecasting engine, you need to train the model using the Python suite located in the `/AI` directory.

```powershell
cd AI
# Install Python dependencies
py -m pip install -r requirements.txt

# Option A: Train with Local Database Data
py train_prophet.py

# Option B: Hybrid Training (DB + Kaggle Data)
py train_prophet.py --file external_data.csv
```

---

## 📊 Dashboard Insights
Tasty Station provides four layers of intelligence:
1. **Historical Analysis**: 7-day revenue and order volume tracking.
2. **Predictive Forecasting**: 30-day AI-generated revenue projections.
3. **Product Rank**: Top/Bottom performing dish identification with growth percentage tracking.
4. **Strategic Actions**: AI-suggested business moves (e.g., Flash Sales, Pricing Adjustments) based on detected trend velocity.

---

## 👨‍💻 Developed By
**Erick** & **Antigravity AI**
*"Building the future of restaurant intelligence, one dish at a time."*
