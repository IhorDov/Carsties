using System;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService_controllers.Models;

namespace SearchService_controllers.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("---> Consuming AuctionDeleted " + context.Message.Id);

        var result = await DB.DeleteAsync<Item>(context.Message.Id);

        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionDeleted), "Problem with deleting auction");
    }
}
