# CNC Controller Project

## Overview

This CNC Controller project is designed to interface with CNC machines, manage configurations, and control machine operations like jogging, homing, connecting, and disconnecting. The application is built in .NET, leveraging MVVM (Model-View-ViewModel) architecture, and integrates services for serial communication, configuration management, and machine control.

## Features

- **Configuration Management**: Load, update, and reset machine settings (e.g., port, baud rate, polling interval) stored in a `config.json` file.
- **Serial Communication**: Connects to CNC machines over serial ports with retry mechanisms.
- **Machine Control**: Commands include:
  - **Connect**: Establish a serial connection.
  - **Disconnect**: Close the serial connection.
  - **Jog**: Control machine movement in specified directions.
  - **Home**: Reset machine to its home position.

## Components

- **CNCViewModel**: The main ViewModel managing UI actions, CNC status, and configuration changes.
- **ConfigurationService**: Loads, updates, and resets configuration settings stored in JSON format.
- **SerialCommService**: Manages the serial port connection, retrying if the connection fails and logging events.
- **Commands**: Includes custom asynchronous relay commands to handle UI actions and trigger services.

## Setup

1. **Clone Repository**:
   ```bash
   git clone <repository_url>
