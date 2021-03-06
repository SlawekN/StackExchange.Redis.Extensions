﻿using System;
using System.IO;
using System.Threading.Tasks;
using MsgPack.Serialization;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MsgPack
{
	public class MsgPackObjectSerializer : ISerializer
	{
		private readonly System.Text.Encoding encoding;

		public MsgPackObjectSerializer(Action<SerializerRepository> customSerializerRegistrar = null, System.Text.Encoding encoding = null)
		{
			if (customSerializerRegistrar != null)
			{
				customSerializerRegistrar(SerializationContext.Default.Serializers);
			}

			if (encoding == null)
			{
				this.encoding = System.Text.Encoding.UTF8;
			}
		}

		public Task<object> DeserializeAsync(byte[] serializedObject)
		{
			return Task.Factory.StartNew(() => Deserialize(serializedObject));
		}

		public T Deserialize<T>(byte[] serializedObject) where T : class
		{
			if (typeof(T) == typeof(string))
			{
				return encoding.GetString(serializedObject) as T;
			}
			var serializer = MessagePackSerializer.Get<T>();

			using (var byteStream = new MemoryStream(serializedObject))
			{
				return serializer.Unpack(byteStream);
			}
		}

		public Task<T> DeserializeAsync<T>(byte[] serializedObject) where T : class
		{
			return Task.Factory.StartNew(() => Deserialize<T>(serializedObject));
		}

		public byte[] Serialize(object item)
		{
			if (item is string)
			{
				return encoding.GetBytes(item.ToString());
			}

			var serializer = MessagePackSerializer.Get(item.GetType());

			using (var byteStream = new MemoryStream())
			{
				serializer.Pack(byteStream, item);

				return byteStream.ToArray();
			}
		}

		public Task<byte[]> SerializeAsync(object item)
		{
			return Task.Factory.StartNew(() => Serialize(item));
		}

		public object Deserialize(byte[] serializedObject)
		{
			return Deserialize<object>(serializedObject);
		}
	}
}
