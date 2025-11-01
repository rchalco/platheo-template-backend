using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VectorStinger.Foundation.Abstractions.Infrastructure
{
    public abstract class BaseAPIExternalService : IExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly string _uri;

        protected BaseAPIExternalService(HttpClient httpClient, string uri)
        {
            _httpClient = httpClient;
            _uri = uri;
        }

        /// <summary>
        /// Realiza una petición POST al servicio externo con el objeto request como cuerpo de la petición.
        /// </summary>
        /// <typeparam name="TRequest">Objeto que contiene los parametros de envio</typeparam>
        /// <typeparam name="TResponse">Objeto resultado de la pericion</typeparam>
        /// <param name="request"></param>
        /// <returns>Result con el objeto de respuesta o error</returns>
        protected async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(TRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(_uri, request);
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var content = await response.Content.ReadFromJsonAsync<TResponse>();
                if (content == null)
                {
                    return Result.Fail<TResponse>("The response content is null.");
                }
                return Result.Ok(content);
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return Result.Ok<TResponse>(default!);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result.Fail<TResponse>($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}. {errorContent}");
            }
        }

        /// <summary>
        /// Realiza una petición GET enviando las propiedades del objeto request como parámetros de query string.
        /// </summary>
        /// <returns>Result con el objeto de respuesta o error</returns>
        protected async Task<Result<TResponse>> GetAsync<TRequest, TResponse>(TRequest request)
        {
            // Construir la query string a partir de las propiedades públicas del objeto request
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (request != null)
            {
                foreach (var prop in typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var value = prop.GetValue(request);
                    if (value != null)
                    {
                        query[prop.Name] = value.ToString();
                    }
                }
            }

            var uriWithQuery = _uri;
            var queryString = query.ToString();
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                uriWithQuery += (uriWithQuery.Contains("?") ? "&" : "?") + queryString;
            }

            var response = await _httpClient.GetAsync(uriWithQuery);
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var content = await response.Content.ReadFromJsonAsync<TResponse>();
                if (content == null)
                {
                    return Result.Fail<TResponse>("The response content is null.");
                }
                return Result.Ok(content);
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return Result.Ok<TResponse>(default!);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result.Fail<TResponse>($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}. {errorContent}");
            }
        }
    }
}
