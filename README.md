# Deleted File Persistence Experiment
This project was created as part of Remote Framing Attacks under Bar-Ilan institute.
The project in high level contains a Windows service agent to run on exprimentors, DB server, automating scripts and few tools. 

The agent communicates with SQL server via php server and is able to commit 3 commands: 
* Run experiment
* Update agent
* Kill yourself

Each time it runs an experiment, it creates a file, delete it after a while and observe his clusters until it compeltely overwritten. It's intended to run on Windows platform running NTFS.
In order to install it on client, just build the ExperimentRunner project using visual studio and install it using installutil of Microsoft.Net or similar as sc util.

The server is a bunch of tables (in "DB tables" folder) and php scripts (in ExperimentCore/PHP Scripts folder).
The mssions for each server are set by the SQL DB. In order to create new experiment, you can use the ExperimentMaker project.

Additionally, there are 3 script tools in the solution:
* RandomWebsiteLoader - Automating user web browsing.
* RandomFilesGenerator - Random traffic on HD.
* FileLocationTool	- Gets the LCN location of given file.


