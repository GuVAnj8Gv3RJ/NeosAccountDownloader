# Neos Account Downloader

A small tool/utility to download your Neos Account contents to your local computer.

# FAQ

## Why does this exist?
Backing up Neos content given the current circumstances seemed wise.

## Can I restore this download into my Neos Account?
No.

## What can I do with the downloaded files?
The files are mostly machine readable collections of entities from your account, feel free to poke around.

You could however, write additional tools that do stuff with them.

## Can I import downloaded content into Unity?
This is not a supported use case of this utility. No effort will be made to support this. You could make your own tooling to do that though.

## Should I use a new folder for each user I download?
Ideally no, the local store that this app builds will in some cases handle duplicate assets in a way that will reduce total file size if you use the same folder for multiple accounts.

## Can I run this app for multiple users at the same time?
Yes, but if you do this, you'll need to use two separate folders which we do not recommend. You may also breach some rate limits Neos has in place on its cloud infrastructure.


# Known Issues

## Localization isn't instant
If you switch languages then the currently active page you're on will not update to the new language. 

Localization defaults to your computer's language, so for most people this hopefully should not be a problem, but for now change your language on the Getting Started screen.

## Progress Metrics aren't 100% Acurate
Neos assets and records are stored in a way that makes it difficult for us to estimate the total number of records required for download. Due to this we sometimes discover more that need to be queued for download as we go. Causing numbers to jump around a little bit.

## Inventory & Worlds appears to be complete but the download is still running
This is most likely queued jobs that need to be finished, or groups contents downloading, see the previous known issue.
