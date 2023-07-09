﻿using EventBus.Base.Events;

namespace NotificationService;

public class OrderPaymentSuccessIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; set; }

    public OrderPaymentSuccessIntegrationEvent(int orderId) => OrderId = orderId;
}
