using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Identity_Service.Logic;

namespace Identity_Service
{
	public class KafkaConsumerHandler : IHostedService
	{
		private readonly string topic = "gdpr_topic";
		private readonly UserHandler userHandler;
		public KafkaConsumerHandler(UserHandler userHandler)
		{
			this.userHandler = userHandler;
		}
		public Task StartAsync(CancellationToken cancellationToken)
		{
			var conf = new ConsumerConfig
			{
				GroupId = "identity_comsumer_group",
				BootstrapServers = "kafka",
				AutoOffsetReset = AutoOffsetReset.Earliest
			};

			using (var builder = new ConsumerBuilder<Ignore, string>(conf).Build())
			{
				builder.Subscribe(topic);
				var cancelToken = new CancellationTokenSource();
				try
				{
					while (true)
					{
						var consumer = builder.Consume(cancelToken.Token);
						Console.WriteLine($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
						userHandler.DeleteUserById(consumer.Message.Value);
					}
				}
				catch (Exception)
				{
					builder.Close();
				}
			}
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
