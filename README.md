# HackerNews
Small .netcore 10 solution getting news from outer WebApi.
Gets information about stories from https://hacker-news.firebaseio.com/

I wanted to keep the solution clean, neat and simple, only keeping the needed parts.
If it was expected to extend - I would split the code on different projects, following the principles of Clean Architecture (Domain Models, services with Application Logic, Repositories and Proxies to connect DBs and third-party api, etc.) Also I would add more logging and metrics, reports to track the state of the system.

Main modules:
- HackerNewsController - contains endpoints to get story, list of best stories and top n stories in descending order.
- HackerNewsApiClient - contains methods to get story data from https://hacker-news.firebaseio.com/
- HackerNewsService - called by HackerNewsController to get data from HackerNewsApiClient. Contains caching logic to save stories data and return it in next requests. Bests stories list is expired and updates each 15 minutes.

Swagger page is created for development and for each main module are added some xUnit tests.
Also mapper and json converter is added for the Story models.

Strategies used to handle high load:
1. Asynchrony used to handle more requests concurrently.
2. In-Memory Caching used to save frequently-called stories data.
3. Retries and CircuitBraker Policy added for hacker-news HttpClient to retry/keep app from calling hacker-news for some time if it returns errors.
4. Semaphore is used to limit the number of concurrent requests to hacker-news.
5. Api is accepting CancellationToken to be able to stop the process, freeing memory and resources.

JMeter load testing on my laptop shows that request to get top 200 stories first time may take up to 10 second.
Next requests use cached data:
 - 50 requests in 3 seconds showed response times mainly around 200-300ms not more than 500ms.
 - 500 requests in 3 seconds showed response times mainly around 2000ms not more than 5000ms.
 - Screenshots from JMeter can be found in repo.
