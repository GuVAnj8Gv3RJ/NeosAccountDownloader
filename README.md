# Neos Account Downloader

A small tool/utility to download your Neos Account contents to your local computer.

# Disclaimer
While every effort is made to download everything from your account, this utility may miss or lose some data. As such we're unable to offer any guarantee or warranty on this application's ability. This is in line with the License but this additional disclaimer is here in the hopes of transparency.

Please refer to the [License](LICENSE.md) file for additional commentary.

# Scope
This repository/software/project aims to do only a limited number of things:
1. Download the contents of a Neos Account and any groups it is part of to a Local Hard Drive.
1. Store the contents in a way that other software or projects can interact with.
1. Preserve that contents in a way that allows peace of mind.

We do not intend to process the contents in a way that makes it compatible with other engines such as Unity, Unreal etc.

# Download Instructions

1. Click [this link](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/releases/latest)
2. Scroll to the section of the page that says "Assets".
3. Click on the Asset(Zip File) that matches your operating system (Windows or Linux) (You **DO NOT** need the source code zips)
4. It will download
5. UnZip the downloaded Zip file
6. Enter the folder from the Zip file
7. Run the Exe (Neos Account Downloader.exe) by double clicking it.

# Contributing
Thanks for your interest in contributing, there are several ways you can contribute!

1. Localization
1. Bug Reporting
1. Feature Requests
1. Engineering / Programming

We appreciate any contributions, you'll get added to our [Contributor](#contributors) list for pretty much any activity.

## Localization
We'd love help with localization.

You have two options when localizing.

### Weblate
We're trying out Weblate for online localization, it will enable you to localize Neos Account Downloader without downloading anything from right in your browser, it is really cool. You can click the image below to go to our location on Weblate.

[![Translation status](https://hosted.weblate.org/widgets/neos-account-downloader/-/neos-account-downloader-resources/287x66-white.png)](https://hosted.weblate.org/engage/neos-account-downloader/)

There's also a [guide](https://docs.weblate.org/en/latest/user/translating.html) for using weblate available. If you're struggling you can always do the manual method as listed below.

#### Weblate Usernames
It is recommended to use the same username and email as the one you have here on GitHub when registering/using Weblate. This will ensure that your changes are accredited to your regular GitHub user when they come across from Weblate.

### Manual
1. Duplicate: https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/blob/main/AccountDownloader/Properties/Resources.resx
1. Save it as Resources.`<your language code>`.resx
1. Translate
1. PR it

## Bug Reporting
All software has bugs, this software included. Whenever a bug is encountered, please [open an issue](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues) the format doesn't really matter provided you clearly explain your problem. Please provide a log file if you can though. We'll get to the bugs as soon as we can.

### Where do I find the log files?
In the top bar menu for the tool click Help -> Open Log Folder. It'll take you right there.

### Can I submit a log file privately?
Sure, email one to: guvanj8gv3rj.undertook642@passinbox.com . This email goes straight to the authors.

## Feature Requests
Feature requests are welcome, please [open an issue](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues) where we can discuss the feature request. Do keep in mind our [scope](#scope) though.

## Engineering / Programming
We're happy to accept contributions in the form of PRs, but please [open an issue](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues) to discuss your changes before making them. We can talk about suitability, method, approach etc.

# FAQ

## How do I download this?
Follow the [download instructions](#DownloadInstructions).

## Why does this exist?
Backing up Neos content given the current circumstances seemed wise.

## Can I restore this download into my Neos Account?
No.

## What can I do with the downloaded files?
The files are mostly machine readable collections of entities from your account, feel free to poke around.

You could however, write additional tools that do stuff with them.

## Can I import downloaded content into Unity?
This is not a supported use case of this utility. No effort will be made to support this. You could make your own tooling to do that though.

## Should I use a new folder for each user/group I download?
Ideally no, the local store that this app builds will in some cases handle duplicate assets in a way that will reduce total file size if you use the same folder for multiple accounts. The download location can handle as many accounts as needed.

## Can I run this app for multiple users at the same time?
Yes, but if you do this, you'll need to use two separate folders which we do not recommend. You may also breach some rate limits Neos has in place on its cloud infrastructure.

## Do subsequent downloads, re-download assets?
For assets, we skip downloading them if an existing asset is found. This makes many downloads incremental rather than starting from scratch.

## Why is assets  showing as 0/XYZ?
For assets, we skip downloading them if an existing asset is found. If your progress statistics or report etc. show 0/xyz etc then it means that no new assets were found.

## What's the difference between Assets and Records/Items/Worlds/Avatars?
This diagram might help:
![](docs/AssetsVsNonAssets.png)

- Assets: Anything that makes up an element in Neos that is not the structure of it within the inspector. So Image,Sounds,Videos,Model Files. These are downloaded incrementally
- Records: Records contain a manifest of all assets that are required to represent an item or world. These are downloaded each time.
- Everything Else: JSON Soup. Just JSON Files of various types. Contacts, Messages etc. These are downloaded each time.

## What are Asset/Record failures?
At the end of your download you might see asset or record failures.

- A record failure usually means that an entire item/world/avatar failed to download, you won't have any assets from that record because the tool could not download the Record to find them
- An asset failure means a part of a downloaded record could not be downloaded.

In both cases, make sure you take a look at the UI scrolling left and right etc. Better UI for this area might be a focus if we continue to see errors. In both events the details are logged to your log files too.

## Why are you anonymous?
I understand that some of you have felt a degree of apprehension due to my remaining anonymous, citing concerns that I'm not an active member of the Neos community. 

This is not the case. I feel the situation surrounding Neos has created some anxiety concerning the future of the projects I've made on this platform, and this downloader is to help mitigate that by providing an opportunity to back up work should the worst come to pass. 

Similarly, other efforts to restore content, such as the Neos wiki edits, have led to account locks for users, denying them the ability to access their assets entirely. This makes it difficult to provide back up options without risking access to the very content it aims to preserve. 

My anonymity is simply intended to provide a basic level of protection. I hope you can find it a not unreasonable one, and I welcome anyone to investigate the Github to verify the contents of the downloader.

You can also read [Verifying Checksums](#verifying-checksumsintegrity) to double check that what you see on GitHub is what you get.

## How can we trust this?
If you'd like to build your trust in this software you can do so in a number of ways:
1. Read the source code of the repository, there is no hidden code or components
1. Read [Verifying Checksums](#verifying-checksumsintegrity) which verifies that the builds you obtain are built on GitHub using code on GitHub without outside interference.
1. Discuss the software with the community, several members of the Neos community including Neos Team members have reviewed this code and confirmed it to be ok.

If you have further doubts feel free to [open an issue](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues) or a [discussion](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/discussions).

## Can we contact the author privately?
Sure you can email: guvanj8gv3rj.undertook642@passinbox.com to conduct a private conversation. Our preference is that conversations are kept public in [issues](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues) or [discussions](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/discussions).

### What about Discord?
We do not respond on Discord and use the account purely to post new versions.

# Verifying Checksums/Integrity

If you'd like to double check the checksums/integrity of the built Binaries of this project. You can follow these steps:
1. Download the latest release(1.10.7 onwards) for your operating system
1. Also download the Checksum for that operating system. It is a text file.
1. Check the [Actions log](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/actions) for this repository, for the version you downloaded
1. Locate the "Print Checksum" step
1. Copy the printed checksum somewhere safe
1. Now you can check the checksum of the downloaded files.
    1. On windows you can run: `certutil -hashfile <file> SHA256`
    1. On Linux you can run: `sha256sum <file>`
1. Compare the output of the command to the downloaded checksum file.
1. Compare the output of the command to the checksum found in the GitHub Actions Log
1. If all 3 match, then you know that this build that you've downloaded was generated purely from GH actions and not manipulated.

This should help to increase trust, that the builds of this tool are not manipulated by the author. What you see on GH is what gets built and served to you via the releases tab.

If you'd like to read more about this concept, you can read this [Wikipedia article](https://en.wikipedia.org/wiki/File_verification).

# Contributors
Below is a list of all of the awesome people who have helped to contribute to this software. If you feel you're missed, or that your [contribution types](https://allcontributors.org/docs/en/emoji-key) aren't correct. Please feel free to [open an issue](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues).

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/TheJebForge"><img src="https://avatars.githubusercontent.com/u/12719947?v=4?s=100" width="100px;" alt="TheJebForge"/><br /><sub><b>TheJebForge</b></sub></a><br /><a href="#translation-TheJebForge" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/orange3134"><img src="https://avatars.githubusercontent.com/u/56525091?v=4?s=100" width="100px;" alt="orange3134"/><br /><sub><b>orange3134</b></sub></a><br /><a href="#translation-orange3134" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/stiefeljackal"><img src="https://avatars.githubusercontent.com/u/20023996?v=4?s=100" width="100px;" alt="Stiefel Jackal"/><br /><sub><b>Stiefel Jackal</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=stiefeljackal" title="Code">💻</a> <a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3Astiefeljackal" title="Bug reports">🐛</a> <a href="#research-stiefeljackal" title="Research">🔬</a> <a href="#ideas-stiefeljackal" title="Ideas, Planning, & Feedback">🤔</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Sharkmare"><img src="https://avatars.githubusercontent.com/u/34294231?v=4?s=100" width="100px;" alt="Sharkmare"/><br /><sub><b>Sharkmare</b></sub></a><br /><a href="#translation-Sharkmare" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/GuVAnj8Gv3RJ"><img src="https://avatars.githubusercontent.com/u/132167543?v=4?s=100" width="100px;" alt="GuVAnj8Gv3RJ"/><br /><sub><b>GuVAnj8Gv3RJ</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=GuVAnj8Gv3RJ" title="Code">💻</a> <a href="#maintenance-GuVAnj8Gv3RJ" title="Maintenance">🚧</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/RileyGuy"><img src="https://avatars.githubusercontent.com/u/9770110?v=4?s=100" width="100px;" alt="Cyro"/><br /><sub><b>Cyro</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=RileyGuy" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Psychpsyo"><img src="https://avatars.githubusercontent.com/u/60073468?v=4?s=100" width="100px;" alt="Psychpsyo"/><br /><sub><b>Psychpsyo</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=Psychpsyo" title="Code">💻</a> <a href="#translation-Psychpsyo" title="Translation">🌍</a></td>
    </tr>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Xlinka"><img src="https://avatars.githubusercontent.com/u/22996716?v=4?s=100" width="100px;" alt="xLinka"/><br /><sub><b>xLinka</b></sub></a><br /><a href="#research-Xlinka" title="Research">🔬</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/bontebok"><img src="https://avatars.githubusercontent.com/u/23562523?v=4?s=100" width="100px;" alt="Rucio"/><br /><sub><b>Rucio</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3Abontebok" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="http://www.xeltalania.com"><img src="https://avatars.githubusercontent.com/u/19335111?v=4?s=100" width="100px;" alt="Samuel-Sann Laurin"/><br /><sub><b>Samuel-Sann Laurin</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3ASectOLT" title="Bug reports">🐛</a> <a href="#translation-SectOLT" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/rampa3"><img src="https://avatars.githubusercontent.com/u/68955305?v=4?s=100" width="100px;" alt="rampa3"/><br /><sub><b>rampa3</b></sub></a><br /><a href="#translation-rampa3" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/ThomFox"><img src="https://avatars.githubusercontent.com/u/137287064?v=4?s=100" width="100px;" alt="ThomFox"/><br /><sub><b>ThomFox</b></sub></a><br /><a href="#research-ThomFox" title="Research">🔬</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/5H4D0W-X"><img src="https://avatars.githubusercontent.com/u/99607717?v=4?s=100" width="100px;" alt="5H4D0W-X"/><br /><sub><b>5H4D0W-X</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3A5H4D0W-X" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="http://probableprime.co.uk"><img src="https://avatars.githubusercontent.com/u/8791132?v=4?s=100" width="100px;" alt="ProbablePrime"/><br /><sub><b>ProbablePrime</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3AProbablePrime" title="Bug reports">🐛</a></td>
    </tr>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/epicEaston197"><img src="https://avatars.githubusercontent.com/u/76523692?v=4?s=100" width="100px;" alt="epicEaston197"/><br /><sub><b>epicEaston197</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3AepicEaston197" title="Bug reports">🐛</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/EIA485"><img src="https://avatars.githubusercontent.com/u/17285570?v=4?s=100" width="100px;" alt="EIA485"/><br /><sub><b>EIA485</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=EIA485" title="Documentation">📖</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/ClimberJ"><img src="https://avatars.githubusercontent.com/u/98227175?v=4?s=100" width="100px;" alt="ClimberJ"/><br /><sub><b>ClimberJ</b></sub></a><br /><a href="#translation-ClimberJ" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/DaichiOkano"><img src="https://avatars.githubusercontent.com/u/49420867?v=4?s=100" width="100px;" alt="DaichiOkano"/><br /><sub><b>DaichiOkano</b></sub></a><br /><a href="#translation-DaichiOkano" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/EuphieEuphoria"><img src="https://avatars.githubusercontent.com/u/101027344?v=4?s=100" width="100px;" alt="EuphieEuphoria"/><br /><sub><b>EuphieEuphoria</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues?q=author%3AEuphieEuphoria" title="Bug reports">🐛</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

## Weblate Contributors
We're still figuring out how to link in Weblate contributors for now, we'll manually add you here :)

* Spanish
    * gallegonovato (6)
