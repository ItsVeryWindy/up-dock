# UpDock
## What's UpDock?
UpDock is a tool for automatically updating docker images within github repositories.

![Build Status](https://github.com/ItsVeryWindy/up-dock/workflows/Build/badge.svg) ![Release Status](https://github.com/ItsVeryWindy/up-dock/workflows/Release/badge.svg)

![Updock Icon](assets/icon.png)

## Command Line Options
```
--auth/-a        Authentication credentials for a docker repository (should be in the form of [registry]=[username],[password])
--config/-c      Default configuration to apply
--dry-run/-d     Run without creating pull requests
--email/-e*      Email to use in the commit
--help/-h        Display help information
--search/-s*     Search query to get repositories
--template/-i    A template to apply
--token/-t       GitHub token to access the repository (can come from standard in)
--version/-v     Display what the version is
```

## Running inside a container
An image has been published to the docker hub for use, as an example.
```
docker run --rm itsverywindy/up-dock:1.0.0 -e ItsVeryWindy@users.noreply.github.com -s repo:ItsVeryWindy/test-docker-update -i "mcr.microsoft.com/dotnet/core/sdk:{v}" -d
```

## Configuration File
Configuartion can either be specified by command line options or via a file for more complex image patterns, by default it will look for a file inside the repository called `up-dock.json` and combine those options with the ones provided.

```
{
    "include": "file/path/**/to_include.xml,
    "exclude": "file/path/**/to_exclude.xml,
    "templates": [
        "example-image",
        {
            "image": "example-repository.com/example-image",
        },
        {
            "pattern": "example-pattern:{v}",
            "image": "example-repository.com/example-image:example-tag{v}",
        }
    ]
}
```
The include and exclude properties use a glob pattern for excluding and including the files that should be scanned.
The templates property is an array of images to search for and update.

There are three different ways of specifying a docker image.
* The first is the short form, which is similar to what you would provide when doing a docker pull
* A slightly longer form, which can be used when you need to specify other paramters
* An even longer form, for when the pattern to match on bares no resemblence to the name of the image

### Image template
An image is a single line of text descibe the image, it follows the same pattern as when doing a docker pull. ie. `[repository]/[image]:[tag]`.
If the tag is not included, it is assumed to be a version number
If the tag contains a `{v}` that is the version number it will try to match on ie. `nginx:{v}`
The version number can also contain a floating number range to match on ie. `nginx:{v1.*}`
Multiple versions can be specified if the image contains more than one version number in the tag ie. `nginx:{v}-alpine{v}`

### Pattern
Sometimes the image you want to update is not fully specified in the place you wish to update it.
For these instances you can specify a pattern that contains just the version numbers, eg. if you had a line in a text file, `NGINX_VERSION=1.0`, you could have the pattern `NGINX_VERSION={v}` with the image `nginx:{v}`.
To be a valid pattern is must have the same number as versions as in the image.

### Grouping
By default, grouping of changes is done based off the image specified. You can specify a `group` property with a string as the value and all changes with that group will be merged into a single pull request.
