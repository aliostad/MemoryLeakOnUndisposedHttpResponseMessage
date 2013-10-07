using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryLeakOnUndisposedHttpResponseMessage
{

    public class InnocentLookingDelegatingHandler : DelegatingHandler
    {

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken)
                .ContinueWith(t =>
                                  {
                                      var response = t.Result;
                                      if (response.StatusCode == HttpStatusCode.NotModified)
                                      {
                                          var cachedResponse = request.CreateResponse(HttpStatusCode.OK);
                                          cachedResponse.Content = new StringContent("Just for display purposes");
                                          return cachedResponse;
                                      }
                                      else
                                        return response;
                                  });
        }   
    }

    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient(new InnocentLookingDelegatingHandler()
                                                {
                                                    InnerHandler = new HttpClientHandler()
                                                });
            for (int i = 0; i < 1000 * 1000; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://ajax.googleapis.com/ajax/libs/angularjs/1.0.7/angular.min.js");
                request.Headers.IfModifiedSince = DateTimeOffset.Now;
                var response = client.SendAsync(request).Result;
                response.Dispose();
                Console.Write("\r" + i);
            }
            
        }
    }
}
