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
	public class KafkaConsumerHandler : BackgroundService
	{
		private readonly string topic = "gdpr_topic";
		private readonly UserHandler userHandler;
		private readonly IConsumer<Ignore, string> kafkaConsumer;
		private readonly ConsumerConfig config;
		public KafkaConsumerHandler(UserHandler userHandler)
		{
			this.userHandler = userHandler;
			config = new ConsumerConfig
			{
				GroupId = "identity_comsumer_group",
				BootstrapServers = "kafka",
				AutoOffsetReset = AutoOffsetReset.Earliest
			};
			this.kafkaConsumer = new ConsumerBuilder<Ignore, string>(config).Build();
		}

		protected override Task ExecuteAsync(CancellationToken cancellationToken)
		{
			new Thread(() => StartConsumerLoop(cancellationToken)).Start();

			return Task.CompletedTask;

			//builder.Subscribe(topic);
			//var cancelToken = new CancellationTokenSource();
			//try
			//{
			//	while (true)
			//	{
			//		var consumer = builder.Consume(cancelToken.Token);
			//		Console.WriteLine($"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
			//		userHandler.DeleteUserById(consumer.Message.Value);
			//	}
			//}
			//catch (Exception)
			//{
			//	builder.Close();
			//}
		}

		private void StartConsumerLoop(CancellationToken cancellationToken)
		{
			kafkaConsumer.Subscribe(this.topic);
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var cr = this.kafkaConsumer.Consume(cancellationToken);
					userHandler.DeleteUserById(cr.Message.Value);
					Console.WriteLine($"{cr.Message.Key}: {cr.Message.Value}ms");
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (ConsumeException e)
				{
					Console.WriteLine($"Consume error: {e.Error.Reason}");

					if (e.Error.IsFatal)
					{
						break;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"Unexpected error: {e}");
				}
			}
		}

		public override void Dispose()
		{
			this.kafkaConsumer.Close();
			this.kafkaConsumer.Dispose();

			base.Dispose();
		}
	}
}
