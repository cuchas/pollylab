using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Polly.Fallback;
using Polly.Timeout;
using System.Threading;

public class CatalogController: ControllerBase {
    private HttpClient _httpClient;

    readonly AsyncTimeoutPolicy _timeoutPolicy;
    readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
    
    readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;

    private int _cachedNumber = 0;

    public CatalogController() {
        //simple retry
        // _httpRetryPolicy = Policy
        // .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        // .RetryAsync(3);

        //simple retry includes timeout
        _httpRetryPolicy = Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .Or<HttpRequestException>()
        .Or<TimeoutRejectedException>()
        .RetryAsync(3, onRetry);

        //simpley retry com funcao quando fail pre retry
        // _httpRetryPolicy = Policy
        // .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        // .RetryAsync(3, (res, retryCounter) => 
        // {
        //     if(res.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
        //         _httpClient = GetHttpClientWithCookie();

        //     }
        // });

        //backoff retry
        //aumenta o delay para o retry por potencia
        // _httpRetryPolicy = Policy
        // .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) 
        // .WaitAndRetryAsync(3, retryTime => TimeSpan.FromSeconds(Math.Pow(2, retryTime) / 2));

        //fallback (cached values on req fails)
        _fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .Or<TimeoutRejectedException>()
        .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("99")
        });

        _timeoutPolicy = Policy.TimeoutAsync(2);
    }

    private void onRetry(DelegateResult<HttpResponseMessage> delegateResult, int retryCounter)
    {
        if(delegateResult.Exception is HttpRequestException) {

        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) 
    {
        _httpClient = GetHttpClient();

        string url = $"inventory/{id}";

        // HttpResponseMessage res = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(url));

        //Chaining Policies Fallback With Retry
        // HttpResponseMessage res = await _fallbackPolicy
        // .ExecuteAsync(
        //     () => _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(url))
        // );

        //Chaining Policies Fallback With Retry and Timeout
        HttpResponseMessage res = await _fallbackPolicy
        .ExecuteAsync(() => _httpRetryPolicy
        .ExecuteAsync(() => _timeoutPolicy
        .ExecuteAsync(() => _httpClient.GetAsync(url))));

        if(res.IsSuccessStatusCode) 
        {
            var body = JsonConvert.DeserializeObject<int>(await res.Content.ReadAsStringAsync());
            return Ok(body);
        }

        return StatusCode((int) res.StatusCode, res.Content.ReadAsStringAsync());
    }

    private HttpClient GetHttpClient() 
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(@"http://localhost:5000/");
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }

    private HttpClient GetHttpClientWithCookie() 
    {
        var uri = new Uri("http://localhost:5000/");
        var cookieContainer = new CookieContainer();
        cookieContainer.Add(uri, new Cookie("auth", "GoodAuthCode"));
        var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        var httpClient = new HttpClient(handler) { BaseAddress = uri };        
        // httpClient.BaseAddress = new Uri(@"http://localhost:5000/");
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }
}