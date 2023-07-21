# Neos Account Downloader

A small tool/utility to download your Neos Account contents to your local computer.

# Disclaimer
While every effort is made to download everything from your account, this utility may miss or lose some data. As such we're unable to offer any guarentee or warranty on this application's ability. This is in line with the License but this additional disclaimer is here in the hopes of transparency.

Please refer to the [License](LICENSE.md) file for additional commentary.

# FAQ

## How do I download this?
Use the [releases tab](https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/releases), grab the latest.

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

## Do subsequent downloads, re-download assets?
For assets, we skip downloading them if an existing asset is found. This makes many downloads incremental rather than starting from scratch.

# Known Issues

## Localization isn't instant
If you switch languages then the currently active page you're on will not update to the new language. 

Localization defaults to your computer's language, so for most people this hopefully should not be a problem, but for now change your language on the Getting Started screen.

## Progress Metrics aren't 100% Acurate
Neos assets and records are stored in a way that makes it difficult for us to estimate the total number of records required for download. Due to this we sometimes discover more that need to be queued for download as we go. Causing numbers to jump around a little bit.

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/TheJebForge"><img src="https://avatars.githubusercontent.com/u/12719947?v=4?s=100" width="100px;" alt="TheJebForge"/><br /><sub><b>TheJebForge</b></sub></a><br /><a href="#translation-TheJebForge" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/orange3134"><img src="https://avatars.githubusercontent.com/u/56525091?v=4?s=100" width="100px;" alt="orange3134"/><br /><sub><b>orange3134</b></sub></a><br /><a href="#translation-orange3134" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/stiefeljackal"><img src="https://avatars.githubusercontent.com/u/20023996?v=4?s=100" width="100px;" alt="Stiefel Jackal"/><br /><sub><b>Stiefel Jackal</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=stiefeljackal" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Sharkmare"><img src="https://avatars.githubusercontent.com/u/34294231?v=4?s=100" width="100px;" alt="Sharkmare"/><br /><sub><b>Sharkmare</b></sub></a><br /><a href="#translation-Sharkmare" title="Translation">🌍</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/GuVAnj8Gv3RJ"><img src="https://avatars.githubusercontent.com/u/132167543?v=4?s=100" width="100px;" alt="GuVAnj8Gv3RJ"/><br /><sub><b>GuVAnj8Gv3RJ</b></sub></a><br /><a href="https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/commits?author=GuVAnj8Gv3RJ" title="Code">💻</a> <a href="#maintenance-GuVAnj8Gv3RJ" title="Maintenance">🚧</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
