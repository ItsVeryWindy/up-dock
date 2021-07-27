# UpDock
## What's UpDock?
UpDock is a tool for automatically updating docker images within github repositories.

![Build Status](https://github.com/ItsVeryWindy/up-dock/workflows/Build/badge.svg) ![Release Status](https://github.com/ItsVeryWindy/up-dock/workflows/Release/badge.svg)

![Updock Icon](assets/icon.png)

## Command Line Options
```
--auth/-a        Authentication credentials for a docker repository (should be in the form of [registry]=[username],[password])
--cache          Cache the results from this run to re-use in another
--config/-c      Default configuration to apply
--dry-run/-d     Run without creating pull requests
--email/-e*      Email to use in the commit
--help/-h        Display help information
--search/-s*     Search query to get repositories
--template/-i    A template to apply
--token/-t       GitHub token to access the repository
--version/-v     Display what the version is

Prefixing the argument with an @ (eg. -@a, --@argument) will signify that value
should come from a line in standard input. Multiple arguments may be specified
this way and will be processed in the order that they appear.
```

## Running inside a container
An image has been published to the docker hub for use, as an example.

```
docker run --rm itsverywindy/up-dock:1.0.0 -e ItsVeryWindy@users.noreply.github.com -s repo:ItsVeryWindy/test-docker-update -i "mcr.microsoft.com/dotnet/core/sdk:{v}" -d
```

## Configuration File
Configuration can either be specified by command line options or via a file for more complex image patterns, by default it will look for a file inside the repository called `up-dock.json` and combine those options with the ones provided.

```json
{
    "include": "file/path/**/to_include.xml",
    "exclude": "file/path/**/to_exclude.xml",
    "templates": [
        "example-image",
        {
            "image": "example-repository.com/example-image",
        },
        {
            "pattern": "example-pattern:{v}",
            "image": "example-repository.com/example-image:example-tag{v}",
            "group": "my-own-group"
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
An image template is a single line of text describes the image to match on, it follows a similar pattern as when doing a docker pull. ie. `[repository]/[image]@[digest]:[tag]`.

#### Repositories
Repository is an optional component and can either reference a third party repository or if not specified assumes the default docker registry.

#### Images
Images should follow the normal docker format.

#### Tags
Tags can be a combination of characters as well as placeholders for the version number.

If the tag contains a `{v}` that is the version number it will try to match on ie. `nginx:{v}` will match `nginx:1.17`.

If the tag is not included it's assumed it is a version number. ie. `nginx` is the same as specifying `nginx:{v}`, just as `nginx@{digest}` is the same as specifying `nginx@{digest}:{v}`.

If the version number can also contain a floating number range to match on ie. `nginx:{v1.*}` would match `nginx:1.17` but not `nginx:2.0`.

Multiple versions can be specified if the image contains more than one version number in the tag ie. `nginx:{v}-alpine{v}` would match `nginx:1.17-alpine1.13`

#### Digests
The digest is an optional component and should only be used when planning to match based on a digest. eg. `nginx@sha256:abcd...`

In this instance the tag is used to lookup which digest to update to.

### Patterns
Sometimes the image you want to update is not fully specified in the place you wish to update it.

For these instances you can specify a pattern that contains just the version numbers or the digest, eg. if you had a line in a text file, `NGINX_VERSION=1.0`, you could have the pattern `NGINX_VERSION={v}` with the image `nginx:{v}`.

Just like image templates, patterns can also contain a floating number range to match on ie. the pattern `NGINX_VERSION={v1.*}` would match a string where the existing text is `NGINX_VERSION=1.0` but not `NGINX_VERSION=2.0`.

To be a valid pattern is must have the same number as versions as in the image template.

#### Digests
A digest can be used in a pattern if the image template also specifies a digest.

In order to specify a digest in a pattern the string must contain `{digest}` ie. `NGINX_VERSION={digest}`.

You can combine digests and versions in the same string, this is useful when trying to keep track of what version the digest is for. ie. `NGINX_VERSION={digest} #{v}` would match `NGINX_VERSION=sha256:abcd... #1.17`.

### Version Match Ordering
In some instances, you may have multiple images that match on the same line of text.

The behaviour in that instance is to match on the last valid entry.

This is more important when you're matching on a general as well as a more specific floating number range ie. `nginx:{v}` as well as `nginx:{v1.*}`

The order of precendence is "Config specified in the repository" > "Config specified on the command line" > "Templates defined on the command line".

There is one exception to this rule if you're redefining an existing entry, where as the existing precedence will still stand ie. if you define an entry for `nginx:{v}`, then another for `nginx:{v1.*}`, and then later on redefine `nginx:{v}` it will still be processed in the original order.

### Grouping
By default, grouping of changes is done based off the image template specified. You can specify a `group` property with a string as the value and all changes with that group will be merged into a single pull request.
