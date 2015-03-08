# aaDataForwarder
A proof of concept for forwarding System Platform data to different consumers

##Just a POC
The current codeset is a pure proof of concept to show an alarm consumer forwarding data to both Splunk and an MQTT broker simultaneously.  Absolutely no effort has been made around proper architecture or speed/resources optimization.

Special thanks to the kind folks at Everdyn for a quick demo on getting data out of the alarm consumer

[http://www.everdyn.com/wonderware-alarming-custom-clients-are-not-that-scary/](http://www.everdyn.com/wonderware-alarming-custom-clients-are-not-that-scary/)

##Futures
###Targets
In the future I plan to expand this to show sending data to services like Twillio, PubNub, and other services as I find them or have time.

###Sources
In terms of source data right now I am demonstrating alams data.  In the future I plan to show examples for mxAccess and Log Files.

## Contributors
* [Andy Robinson](mailto:andy@phase2automation.com), Principal of [Phase 2 Automation](http://phase2automation.com).

## License
MIT License. See the [LICENSE file](/license) for details.
