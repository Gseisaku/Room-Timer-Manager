# Multi-Room Timer Management System Specification

## 1. Overview

This application is a system for simultaneously managing timers for multiple rooms, recording and storing session data, and generating Excel reports at the end of the year.

- **Application Name:** Multi-Room Timer Management System
- **Version:** Ver.3.1

## 2. Main Features

### 2.1. Main Screen

- **Layout:** A grid layout that displays a list of 19 rooms.
- **Version Display:** The current application version is displayed in the lower right corner of the screen.
- **Report Generation:** Provides a function to generate an annual report based on all session data.

### 2.2. Room Management Functions

Each room is displayed as an independent card and has the following functions.

#### 2.2.1. Display Items

- **Room Number:** A number to identify each room.
- **Timer:**
    - Displays the remaining time in `minutes:seconds` format.
    - If the scheduled time is exceeded, the overtime is displayed in `+minutes:seconds` format with a reddish background color.
- **Scheduled End Time:**
    - Displayed to the right of the timer display in `( ~ hour:minute)` format while the timer is running.
- **Status Display:**
    - The background color of the card changes according to the timer's status.
        - **Available:** Light Gray
        - **Running:** Light Green
        - **Warning:** Yellow when less than 10 minutes remain
        - **Paused:** Light Blue
        - **Finished/Overtime:** Salmon Pink

#### 2.2.2. Input Items

Input is possible only when the timer is in the available state.

- **Cast:** A text box to enter the cast name.
- **Course (min):** Radio buttons to select the session duration (45, 60, 70, 90, 120 minutes).
- **Type:** Radio buttons to select the session type (F, H, N).

#### 2.2.3. Operation Buttons

- **Start:** Starts the timer. Can only be pressed when **all three "Cast", "Course", and "Type" are entered/selected**.
- **Pause / Resume:** Toggles between pausing and resuming the timer.
- **+30:** Extends the remaining time by 30 minutes while the timer is **paused**.
- **End:** Stops the timer, saves the session data, and returns the room to the available state.
- **Reset:** Stops the timer without saving the session data, clears all inputs, and returns to the available state.

## 3. Data Storage Function

- **Save Timing:** The session data for that room is saved when the "End" button is pressed.
- **Save Location:** The `SessionData` directory in the application's execution folder.
- **File Format:** Saved in JSON format with the file name `YYYY-MM-DD.json` for each date.

## 4. Report Generation Function

- **Execution Timing:** Executed when the "Generate Yearly Report" button in the lower right corner of the main screen is pressed.
- **Activation Condition:** The button is enabled when there is at least one saved session data.
- **Output Location:** A `Reports` directory is automatically created in the application's execution folder, and the report is saved there.
- **File Name:** An Excel report for the executed year is generated in the format `TimerReport_{year}.xlsx`.
- **File Contents:**
    - Extracts only the data for the executed year from the saved data.
    - An independent worksheet is created for each date for which data exists.
    - The following items are output to each worksheet.

| No. | Column Name    | Content                                                              |
|:----|:---------------|:---------------------------------------------------------------------|
| 1   | Seq            | A sequential number starting from 1 that resets for each date.       |
| 2   | Room Number    | The room number.                                                     |
| 3   | Cast Name      | The entered cast name.                                               |
| 4   | Course (min)   | The selected course duration (in minutes).                           |
| 5   | Type           | The selected session type (F, H, N).                                 |
| 6   | Start Time     | The session start time (`HH:mm:ss`).                                 |
| 7   | End Time       | The actual end time when the "End" button was pressed (`HH:mm:ss`).  |
| 8   | Sched EndTime  | The originally scheduled end time (`HH:mm:ss`).                      |
| 9   | Overtime       | The time exceeded beyond the scheduled time (`hh:mm:ss`). Blank if no overtime. |
| 10  | Remarks        | A blank column for manual remarks.                                   |
