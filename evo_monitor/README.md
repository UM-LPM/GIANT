# Bandwidth Calculator

This web application helps calculate bandwidth requirements based on data sourced from a Google Sheet. Follow the steps below to set up and run the application locally.

---

## Table of Contents

- [Prerequisites](#prerequisites)

- [Setup Instructions](#setup-instructions)

- [Running the Application](#running-the-application)

- [Environment Variables](#environment-variables)
---

## Prerequisites

Before setting up the application, ensure you have the following installed:

1.  **Node.js**: Download and install Node.js from [https://nodejs.org](https://nodejs.org).
---

## Setup Instructions

1.  **Clone the Repository**

	```bash
	git clone ssh://git@gitlab.dewesoft.com:2345/Automotive/LPM/bandwidthcalculator.git
	cd bandwidthcalculator
	```

2.  **Create a `.env` File**
Create a `.env` file in the project root directory and add the following properties:
	```
	VITE_GOOGLE_SHEET_ID='' 
	VITE_GOOGLE_SHEET_NAME_INPUTS='Inputs' 	
	VITE_GOOGLE_SHEET_NAME_CALC_LIMITS='Calculations_limits' 
	VITE_GOOGLE_SHEET_NAME_SPECIAL_CASES='Special_cases' 
	VITE_GOOGLE_SHEET_RANGE_INPUTS='A1:B100' 
	VITE_GOOGLE_SHEET_RANGE_CALC_LIMITS='A1:O100' 
	VITE_GOOGLE_SHEET_RANGE_SPECIAL_CASES='A1:Q100' 
	VITE_GOOGLE_SHEET_URL='https://sheets.googleapis.com/v4/spreadsheets' 
	VITE_GOOGLE_API_KEY=''
	```
3.  **Install Dependencies**
Install all required packages that application requires with the following command:
	```
	npm install
	```

## Running the Application
-   Navigate to the `bandwidthCalculator` directory:
	```
	cd bandwidthCalculator
	```
-   Start the development server:
    ```
	npm run dev
	```
-   Open your web browser and navigate to the URL provided in the browser.
    ```
	http://localhost:3000
	```
## Environment Variables
The application relies on several environment variables to connect with Google Sheets:

| **Variable**                        | **Description**                                  |
|-------------------------------------|--------------------------------------------------|
| `VITE_GOOGLE_SHEET_ID`              | The ID of the Google Sheet.                     |
| `VITE_GOOGLE_SHEET_NAME_INPUTS`     | Name of the "Inputs" sheet.                     |
| `VITE_GOOGLE_SHEET_NAME_CALC_LIMITS`| Name of the "Calculations_limits" sheet.        |
| `VITE_GOOGLE_SHEET_NAME_SPECIAL_CASES`| Name of the "Special_cases" sheet.            |
| `VITE_GOOGLE_SHEET_RANGE_INPUTS`    | Range of the "Inputs" sheet (e.g., A1:B100).    |
| `VITE_GOOGLE_SHEET_RANGE_CALC_LIMITS`| Range of the "Calculations_limits" sheet.      |
| `VITE_GOOGLE_SHEET_RANGE_SPECIAL_CASES`| Range of the "Special_cases" sheet.           |
| `VITE_GOOGLE_SHEET_URL`             | Base URL for Google Sheets API.                 |
| `VITE_GOOGLE_API_KEY`               | Your Google API key.                            |