﻿using Microservices.Web.Models;
using Microservices.Web.Service.IService;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Microservices.Web.Utility.SD;

namespace Microservices.Web.Service
{
	public class BaseService : IBaseService
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public BaseService(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
		{
			try
			{
				// Tạo client và chuẩn bị HttpRequestMessage với Header chấp nhận JSON
				HttpClient client = _httpClientFactory.CreateClient("MicroservicesAPI");
				HttpRequestMessage message = new();
				message.Headers.Add("Accept", "application/json");
				//token

				// Gán URL API cần gọi, gắn nội dung JSON vào Content
				message.RequestUri = new Uri(requestDto.Url);
				if (requestDto.Data != null)
				{
					message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
				}

				HttpResponseMessage? apiResponse = null;

				switch(requestDto.ApiType)
				{
					case ApiType.POST:
						message.Method = HttpMethod.Post;
						break;
					case ApiType.DELETE:
						message.Method = HttpMethod.Delete;
						break;
					case ApiType.PUT:
						message.Method = HttpMethod.Put;
						break;
					default:
						message.Method = HttpMethod.Get;
						break;
				}

				// Gửi request đến API và chờ lấy kết quả
				apiResponse = await client.SendAsync(message);

				switch (apiResponse.StatusCode)
				{
					case HttpStatusCode.NotFound:
						return new() { IsSuccess = false, Message = "Not Found" };
					case HttpStatusCode.Forbidden:
						return new() { IsSuccess = false, Message = "Access Denied" };
					case HttpStatusCode.Unauthorized:
						return new() { IsSuccess = false, Message = "Unauthorized" };
					case HttpStatusCode.InternalServerError:
						return new() { IsSuccess = false, Message = "InternalServerError" };
					default:
						var apiContent = await apiResponse.Content.ReadAsStringAsync();
						var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
						return apiResponseDto;
				}
			} catch (Exception ex)
			{
				var dto = new ResponseDto
				{
					Message = ex.Message.ToString(),
					IsSuccess = false
				};
				return dto;
			}
		}
	}
}
