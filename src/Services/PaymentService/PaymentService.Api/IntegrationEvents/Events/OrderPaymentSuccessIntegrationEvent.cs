﻿using EventBus.Base.Events;

namespace PaymentService.Api;

public class OrderPaymentSuccessIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; set; }

    public OrderPaymentSuccessIntegrationEvent(int orderId) => OrderId = orderId;
}
