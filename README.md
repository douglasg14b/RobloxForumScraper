# RobloxForumScraper

Multithreaded scraper for the Roblox forums. This was created by request.

## Config

`scaper.ini` is the config file. Parameters:

* **MaxDownloaders**: The number of download workers you wish to have, scale this up till you saturate your network bandwidth.
* **MaxProcessors**: The number of processing workers you will use. There are some issues with the HTML parsing library I'm using that prevents any performance gains from more than 1 worker, as the library already attempts to use multiple threads.
* **StartThread**: The ID of the thread you wish to start processing from
* **MaxThread**: The maximum thread ID, where you want the scraper to stop
* **ThreadsBeforeWrite**: The number of threads you wish to process before writing to the database
* **PullEmptyThreads**: `true` or `false`. If you wish to try and repull threads that came back empty or as errors. This option was made after an error caused a few million threads to be skipped.

You don't need to worry about getting the number of downloaders and processors just right so they are synced on performance. The processors will pause if they don't have work to do, and the downloaders will pause of the DB queue goes 1.5x over the write amount.

## Performance

On an old `i7-2600` I pull and process ~125 threads/second or 1 thread every 8ms.

## Screenshot

![alt text](https://raw.githubusercontent.com/douglasg14b/RobloxForumScraper/master/a7RR0Kq.png)

