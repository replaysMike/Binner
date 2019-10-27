# Binner
Binner is a free open-source parts inventory tracking system you can run locally in Windows or Unix, or online at [Binner.io (launches in Dec 2019)](http://binner.io). It was created for makers (like myself), hobbyists or electronic engineers to keep track of your local parts inventory.

# Screenshot

![Binner](https://github.com/replaysMike/Binner/wiki/binner.png)

## Features
* Standard inventory management input
* Import your orders from Digikey, Mouser or ~AliExpress~
* Customizable nested category placement
* Automated datasheet retrieval / Datasheet search
* Automated part lookup on Digikey/Mouser
* Flexible search engine
* Export your data to CSV / Excel if you need
* Proprietary file-based database (or use providers for SQL Express, MySQL, Postgresql)
* No web server installation required, standalone Kestrel service API
* Based on .Net Core - runs on Windows and Unix
* UI available for Windows and Web browsers

## Planned Upcoming Features
* Barcoding support
* Barcode scanning
* Dedicated datasheet repository
* Label printing
* Maybe a parts marketplace?

## Install

Download the latest release for your environment (available Oct 31 2019)

## Description

Binner is in active development and nearing release. It is designed for electronics in mind, however it could be used for other types of inventory management (chemistry, retail). It is purpose built for quick data entry and fast performance. I built it because I couldn't find good free alternatives for tracking inventory in my home maker lab - it's easy to forget what you bought and where you put them and I end up sometimes ordering things I already have around. Saves money and time! I welcome all who want to contribute to this project, and please suggest features in the Issues section.

## Integrations

Currently supports _Digikey_, _Mouser_ and ~_AliExpress_~. For standalone installations, you will need to obtain your own API keys to enable these features. It's easy to do and instructions will be documented in the Wiki.
