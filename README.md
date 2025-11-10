# emby.extract.strm.data

Populate media stream data from strm files using internal Emby's functionality.

## Available functionality

* Scheduled task (Extract Strm Data) to extract media stream data from strm files regularly. There is no scheduled time for this task by default.
* Monitoring for a new strm file. Turned off by default.

## Install

* Pull the dll file into the plugin's folder.

## Setup

* Select in setting on what libraries you want to execute the scheduled task.
* Turn on new file monitoring.

## Usage

1. Select relevant libraries in the plugin's settings.
2. Schedule a task for repeatable execution or invoke the task manually.
3. Turn on new file monitoring in settings.

## Requirements

* The plugin was tested on version 4.9.1.80
* Compiled using .Net 9.0 for .NetStandard 2.0
