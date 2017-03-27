﻿namespace SmartyStreets.USStreetApi
{
	using System;
	using System.Text;
	using NUnit.Framework;

	[TestFixture]
	public class ClientTests
	{
		#region [ Single Lookup ]

		[Test]
		public void TestSendingSingleFreeformLookup()
		{
			var sender = new RequestCapturingSender();
			var serializer = new FakeSerializer(null);
			var client = new Client("http://localhost/", sender, serializer);

			client.Send(new Lookup("freeform"));

			Assert.AreEqual("http://localhost/?street=freeform", sender.Request.GetUrl());
		}

		[Test]
		public void TestSendingSingleFullyPopulatedLookup()
		{
			var sender = new RequestCapturingSender();
			var serializer = new FakeSerializer(null);
			var client = new Client("http://localhost/", sender, serializer);
			var lookup = new Lookup
			{
				Addressee = "0",
				Street = "1",
				Secondary = "2",
				Street2 = "3",
				Urbanization = "4",
				City = "5",
				State = "6",
				ZipCode = "7",
				Lastline = "8",
				MaxCandidates = 9,
				MatchStrategy = "10"
			};

			client.Send(lookup);

			Assert.AreEqual("http://localhost/?street=1&street2=3&secondary=2&city=5&state=6&zipcode=7&lastline=8&addressee=0&urbanization=4&match=10&candidates=9", sender.Request.GetUrl());
		}

		#endregion

		#region [ Batch Lookup ]

		[Test]
		public void TestEmptyBatchNotSent()
		{
			var sender = new RequestCapturingSender();
			var client = new Client("/", sender, null);

			client.Send(new Batch());

			Assert.Null(sender.Request);
		}

		[Test]
		public void TestSuccessfullySendsBatchOfAddressLookups()
		{
			var sender = new RequestCapturingSender();
			var expectedPayload = Encoding.ASCII.GetBytes("Hello World!");
			var serializer = new FakeSerializer(expectedPayload);
			var client = new Client("http://localhost/", sender, serializer);
			var batch = new Batch();
			batch.Add(new Lookup());
			batch.Add(new Lookup());

			client.Send(batch);

			Assert.AreEqual(expectedPayload, sender.Request.Payload);
		}

		#endregion

		#region [ Response Handling ]

		[Test]
		public void TestDeserializeCalledWithResponseBody()
		{
			var response = new Response(0, Encoding.ASCII.GetBytes("Hello, world!"));
			var sender = new MockSender(response);
			var deserializer = new FakeDeserializer(null);
			var client = new Client("/", sender, deserializer);

			client.Send(new Lookup());

			Assert.AreEqual(response.Payload, deserializer.Payload);
		}

		[Test]
		public void TestCandidatesCorrectlyAssignedToCorrespondingLookup()
		{
			var expectedCandidates = new Candidate[3];
			expectedCandidates[0] = new Candidate(0);
			expectedCandidates[1] = new Candidate(1);
			expectedCandidates[2] = new Candidate(1);
			var batch = new Batch();
			batch.Add(new Lookup());
			batch.Add(new Lookup());

			var sender = new MockSender(new Response(0, new Byte[0]));
			var deserializer = new FakeDeserializer(expectedCandidates);
			var client = new Client("/", sender, deserializer);

			client.Send(batch);

			Assert.AreEqual(expectedCandidates[0], batch[0].Result[0]);
			Assert.AreEqual(expectedCandidates[1], batch[1].Result[0]);
			Assert.AreEqual(expectedCandidates[2], batch[1].Result[1]);
		}

		#endregion
	}
}