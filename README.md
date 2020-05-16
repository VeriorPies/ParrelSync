**Note: Readme are still under contruction**
# ParrelSync 
[![Release](https://img.shields.io/github/v/release/VeriorPies/ParrelSync?include_prereleases)](https://github.com/VeriorPies/ParrelSync/releases) [![Documentation](https://img.shields.io/badge/documentation-brightgreen.svg)](https://github.com/VeriorPies/ParrelSync/wiki) [![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/VeriorPies/ParrelSync/blob/master/LICENSE.md) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/VeriorPies/ParrelSync/pulls) [![Downloads](https://img.shields.io/github/downloads/VeriorPies/ParrelSync/total)](https://github.com/VeriorPies/ParrelSync/releases) [![Downloads](https://img.shields.io/discord/710688100996743200)](https://discord.gg/TmQk2qG)  

ParrelSync is a Unity editor extension for improving multiplayer testing workflow.  
<br>
ParrelSync allows users to test multiplayer without building by open multiple editor instances of the same project and have it running as another clients/server, which significantly improved the multiplayer testing workflow since more stats can be monitored/changed from the editor windows and also save a lot of time from building the project.

<br>

![ShortGif](https://raw.githubusercontent.com/VeriorPies/ParrelSync/master/Images/Showcase%201.gif)
<p align="center">
<b>Test project changes on clients and server within seconds - both in editor
</b>
<br>
</p>

## Installation 
To install *ParrelSync*, download .unitypackage from the [latest release](https://github.com/VeriorPies/ParrelSync/releases) and import it to your project.  
Parrel Sync should appreared in the menu item bar.

***Note:*** 
*It's always recommend to backup the project before importing ParrelSync.*

## Supported Platform
Currently, ParrelSync only support Windows editor.  
Please create a [feature request](https://github.com/VeriorPies/ParrelSync/issues/new/choose) if you want Mac/Linux support to be added. 

ParrelSync has been tested with the following Unity version. However, it should also work with other versions as well.
* *2019.3.0f6*
* *2018.4.22f1*


## APIs
Except the handy user interface, ParrelSync also provide some useful APIs for improving multiplayer testing workflow.

Here's a basic example: 
```
if (ClonesManager.IsClone()) {
  // Automatically connect to localhost if running in clone instance
}
```
Check out [the doc](https://github.com/VeriorPies/ParrelSync/wiki/List-of-APIs) to view the complete API list.

## How does it work?
In able to open the project with different editor isntances, for each instance, ParrelSync create a "clone" of the original project and refering the ```Asset```, ```Packages``` and ```ProjectSettings``` folder back to the  original with  [symbolic link](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/mklink), so all the asset in the "clones" will be same as the original one. Asset serialization and saving are also been disabled on the cloned instances for proecting the original assets.  
All cloned project folders are placed right next to the original project with suffix *```_clone_x```*, which should be something like this in the folder hierarchy. 
```
/ProjectName
/ProjectName_clone_0
/ProjectName_clone_1
...
```
## Discord Server
We have a [Discord server](https://discord.gg/TmQk2qG).

## Need Help?
Some common question and troubleshooting can be found under the [Troubleshooting & FAQs](https://github.com/VeriorPies/ParrelSync/wiki/Troubleshooting-&-FAQs) page.  
You can also [create a question post](https://github.com/VeriorPies/ParrelSync/issues/new/choose), or ask on [Discord](https://discord.gg/TmQk2qG) if you prefer to have a real-time conversation.

## Support this project 
A star will be appreciated :)

## Credits
This project is based on hwaet's [UnityProjectCloner](https://github.com/hwaet/UnityProjectCloner) , with bugs fixed and many more features added.
