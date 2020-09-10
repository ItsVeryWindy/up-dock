# docker-upgrade-tool
Automatically update docker images in github repositories.

## Command Line Options
```
  --email/-e      The email address to use in the commit
  --token/-t      GitHub token to access the repository
  --search/-s     Search query to get repositories
  --config/-c     Default configuration file to apply
  --template/-t*  A template string for the docker image to be updated
  --auth/-a*      Authentications details for a docker repository (Should be in the form of [registry]=[username],[password])
```

## Configuration File
Configuartion can either be specified by command line options or via a file for more complex image patterns, by default it will look for a file inside the repository called `docker_images.json` and combine those options with the ones provided.

```
{
    "include": "file/path/**/to_include.xml,
    "exclude": "file/path/**/to_exclude.xml,
    "templates": [
        "example-image",
        {
            "image": "example-image",
            "repository": "example-repository.com"
        },
        {
            "pattern": "example-pattern:{v}",
            "image": "example-image:example-tag{v}",
            "repository": "example-repository.com"
        }
    ]
}
```
The include and exclude properties use a glob pattern for excluding and including the files that should be scanned.
The templates property is an array of images to search for and update.

There are three different ways of specifying a docker image.
* The first is the short form, which is similar to what you would provide when doing a docker pull
* A slightly longer form, which can be used when the string being matched on only contains the name and not the repository it's coming from
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
