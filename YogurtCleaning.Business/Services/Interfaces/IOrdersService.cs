﻿using YogurtCleaning.Business.Models;
using YogurtCleaning.DataLayer.Entities;

namespace YogurtCleaning.Business.Services;

public interface IOrdersService
{
    public interface IOrdersService
    {
        Order? GetOrder(int id, UserValues userValues);
        void UpdateOrder(Order modelToUpdate, int id);
        void DeleteOrder(int id, UserValues userValues);
        List<Order> GetAllOrders();
    int AddOrder(OrderBusinessModel order);
    }
}