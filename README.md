# FeedMD

FeedMD is a CLI Utility used to create a personal daily digest of your favourite news sources by converting RSS and Atom feeds into markdown files. These files can read in your markdown viewer of choice such as [Obsidian](https://obsidian.md/) or used as part of a static site build process such as [Hugo](https://gohugo.io/).

## Features

* Convert RSS/ Atom feeds to markdown files
* Ability to template to modify the markdown output to fit your particular workflow
* Specify both the date and the timezone to generate the daily digest for
* Only parses as much of the document as needed for the given timeframe

## Installation

Download the latest release for your platform here: [https://github.com/myquay/feedmd/releases](https://github.com/myquay/feedmd/releases) or clone the repository and compile for your target system. Run it as you would any other self-contained executable.

## Configuration

Before you generate your first digest, you need to configure which feeds you are interested in. This is configured in a file called `config.yml`.

Example config file

```yml
feeds:
  - https://brandur.org/articles.atom
  - https://michael-mckenna.com/blog/atom.xml
  - https://waitbutwhy.com/feed
  - https://www.joelonsoftware.com/feed/
  - https://jvns.ca/atom.xml
  - https://feeds.hanselman.com/ScottHanselman
  - https://blog.bonnieeisenman.com/feed.xml
time_zone: Pacific/Auckland
```

* `feeds:` Array of feeds to process
* `time_zone:` The time zone used to define the time boundaries for the given date

If you don't want to create the files from scratch you can run the `init` command to generate a sample config and template file (`feedmd init`).

## Generating the digest

Use the `build` command to generate the digest, by default it will generate for the day prior (`feedmd build`).

This will output a file called `{digest_date}.md` with a summary of all new items from the feeds for that date.

Some common configuration options for the build command are:

* `--date`: Specify the date to generate the digest for, in yyyy-mm-dd format e.g. (2023-07-15)
* `-d` or `--destination`: Location to output the digest files to

See the reference section at the end of this document for a full list of commands and options

## Templating

FeedMD supports templating to modify the output of the markdown file, this is to give the tool flexibility to fit into different workflows. 

The [Liquid templating language](https://shopify.github.io/liquid/basics/introduction/) is used, the context available follows this structure:

```csharp
{
  Date: "2023-07-15",
  Feeds: [{
    Title: "Feed title",   //String of name of feed
    Link: "Website link",  //Uri of link to website
    LastModified: ...      //DateTime when feed was last modified
    Items:[{
      Title: "Item title", //String of name of item, e.g. title of blog post
      Link: "Item link",   //Uri of link to item, e.g. link to blog post
      Summary: "Item summary", //String of summary of item e.g. blog post summary
      Content: "Item content", //String of content of item e.g. blog post content
      PublishDate: ...     //DateTime when item was published e.g. blog post publish date
    }]
  }]
}
```

### Example template

This is the template used by default, can be modified by running `feedmd init` and modifiying the generated `template.tmd` file.

```liquid
---
publishDate: {{ Date }}
title: 'RSS Daily Digest: {{ Date }}'
url: /digest/{{ Date }}
---

{% for feed in Feeds -%}
  ### [{{ feed.Title }}]({{ feed.Link }})

  {% for item in feed.Items -%}
	* [{{ item.Title }}]({{ item.Link }})
  {% endfor %}
{% else -%}
  _No feeds today!_
{% endfor %}
```

## Usage Examples

I'm using this to generate the diests for my [RSS reader](https://reader.michael-mckenna.com/) which you [can view the source for here](https://github.com/myquay/reader.michael-mckenna.com). A GitHub action runs FeedMD overnight which generates a new markdown file and saves it to the reader repository. The reader is a markdown website which is regenerated and deployed each time a new digest is created.

### Other usage examples

Let me know what other interesting uses you find for me and I can place them here.

## Reference

Full reference for the FeedMD utility

### Options for the build command

* `--date`: Specify the date to generate the digest for, in yyyy-mm-dd format e.g. (2023-07-15)
* `-d` or `--destination`: Location to output the digest files to
* `-c` or `--configuration`: Location of the configuration file, defaults to `./config.yml` - fails if not present
* `-t` or `--template`: Location of the template file, defaults to `./template.tmd` - if not present uses a default template saved as an embedded resource in the executable.
* `--tz`: Time zone, overrides the one defined in the `config.yml` file
* `-v` or `--verbose`: Generate more output about what's going on
* `--strict`: Parse in strict mode (e.g. do not process DTDs), defaults to `false`
* `--maxDtdCharacters`: Maximum number of characters to read when parsing DTDs, defaults to `1024`
