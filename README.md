# Nexus Mods File Uploader

> Because a real API will be ready ***Soon™***

## Introduction

This is a small command-line tool that can be used to upload mod files to an existing mod on Nexus Mods.

This is **very** unofficial and **very** unsupported. If you want to see an official and supported API for mod authors, open a topic on [the forums](https://forums.nexusmods.com/index.php?/forum/117-feedback-suggestions-and-questions/) or in `#site-help-feedback` on Discord to let Nexus know the demand.

This tool does not automate the web UI (as an [existing solution](https://github.com/BUTR/Bannerlord.NexusmodsUploader) already does a perfectly good job of this (👋 Aragas)), but instead recreates the underlying requests to upload a file and add it to an existing mod. It also supports adding/updating changelogs, but this hasn't been tested much.

## Configuration and Usage

Since there's a lot of information required when updating mods, the recommended method is to create a configuration file for non-sensitive information and use environment variables just for the sensitive information. That being said, you can mix-and-match keys from the config file or environment variables (just prefix them with `UNEX_`) at will.

Sample `unex.yml`:

```yaml
Game: site
ModId: 163
FileName: Upload Test
FileDescription: |-
  Your file description should go here.

  You can include whatever you like in your description, it's added as-is.
```

The remaining two configuration keys are sensitive and should not be made public, but you need both of them (for hard-to-explain reasons): a valid API key and session cookies. Your API key should be in the `UNEX_APIKEY` environment variable. Cookies can be provided in two ways:

- If you have the raw Cookie header from a valid session, you can include the whole header in the `UNEX_COOKIES` variable
- If you have an exported `cookies.txt` file, you can include the relative path to the file in the `UNEX_COOKIES` variable (like `./cookies.txt`)

> All relative paths will be parsed relative to the *current working directory*

Then, call the CLI to begin your upload:

```bash
#unex upload <mod-id> <path-to-file> -v <target-version>
unex upload 163 ./Your-Mod-File.zip -v 1.1.2
```

> By default, your mod's main version will be updated to your new file version, but you can skip this using the `--no-version-update` option. 

You can optionally upload your file as a replacement for an existing file, by providing the `PreviousFile` configuration key. Set it to a file ID to directly replace that file, or to `"auto"` to replace the highest-versioned Main File on your mod (this is both highly experimental and only available for published mods).