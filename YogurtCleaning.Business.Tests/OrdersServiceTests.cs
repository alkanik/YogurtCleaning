﻿using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YogurtCleaning.Business.Models;
using YogurtCleaning.Business.Services;
using YogurtCleaning.DataLayer.Entities;
using YogurtCleaning.DataLayer.Enums;
using YogurtCleaning.DataLayer.Repositories;

namespace YogurtCleaning.Business.Tests;

public class OrdersServiceTests
{
    private Mock<IOrdersRepository> _mockOrdersRepository;
    private Mock<ICleanersService> _mockCleanersService;
    private Mock<IClientsRepository> _mockClientsRepository;
    private Mock<IBundlesRepository> _mockBundlesRepository;
    private Mock<IEmailSender> _mockEmailSender;
    private IMapper _mapper;
    private OrdersService _sut;


    public OrdersServiceTests()
    {
        _mockClientsRepository = new Mock<IClientsRepository>();
        _mockCleanersService = new Mock<ICleanersService>();
        _mockOrdersRepository = new Mock<IOrdersRepository>();
        _mockBundlesRepository = new Mock<IBundlesRepository>();
        _mockEmailSender = new Mock<IEmailSender>();
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new BusinessMapperConfigStorage());
        });
        _mapper = mapper.CreateMapper();
        _sut = new OrdersService(_mockOrdersRepository.Object, 
            _mockCleanersService.Object, 
            _mockClientsRepository.Object, 
            _mockBundlesRepository.Object, 
            _mockEmailSender.Object, _mapper);
    }

    [Fact]
    public void UpdateOrder_WhenUpdatePassed_ThenPropertiesValuesChandged()
    {
        // given

        var order = new Order
        {
            Id = 10,
            Client = new() { Id = 11},
            CleaningObject = new() { Id = 56},
            Status = Status.Created,
            StartTime = DateTime.Now.AddDays(1),
            Bundles = new List<Bundle> { new Bundle { Id = 2, Name = "qwe" } },
            Services = null,
            CleanersBand = new List<Cleaner> {new() { Id = 654 } },
            IsDeleted = false
        };

        var updatedOrder = new Order
        {
            Status = Status.Edited,
            StartTime = DateTime.Now.AddDays(2),
            UpdateTime = DateTime.Now,
            Bundles = new List<Bundle> { new() { Id = 2, Name = "qwe" }, new() { Id = 22, Name = "qwa" } },
            Services = new List<Service> { new Service { Id = 3456} },
            CleanersBand = new List<Cleaner> { new() { Id = 654 }, new() { Id = 777} }
        };

        _mockOrdersRepository.Setup(o => o.GetOrder(order.Id)).Returns(order);

        // when
        _sut.UpdateOrder(updatedOrder, order.Id);

        // then
        _mockOrdersRepository.Verify(o => o.UpdateOrder(
            It.Is<Order>(
                i => i.Id == order.Id
                && i.Client == order.Client
                && i.CleaningObject == order.CleaningObject
                && i.Status == updatedOrder.Status
                && i.StartTime == updatedOrder.StartTime
                && i.UpdateTime == updatedOrder.UpdateTime
                && i.Bundles == updatedOrder.Bundles
                && i.Services == updatedOrder.Services
                && i.CleanersBand == updatedOrder.CleanersBand)),
            Times.Once);
        _mockOrdersRepository.Verify(o => o.GetOrder(order.Id), Times.Once);
    }

    [Fact]
    public void AddOrderTest_WhenCleanersIsEnough_ThenOrderStatusIsCreated()
    {
        // given
        var cleaners = new List<Cleaner>
        {
            new Cleaner()
            {
                Id = 11,
                Schedule = Schedule.FullTime,
                Orders = new List<Order>(),
                DateOfStartWork = new DateTime(2022, 8, 1, 00, 00, 00)
            },
            new Cleaner()
            {
                Id = 13,
                Schedule = Schedule.ShiftWork,
                Orders = new List<Order>(),
                DateOfStartWork = new DateTime(2022, 8, 1, 00, 00, 00)
            }
        };

        var order = new OrderBusinessModel
        {
            Client = new() { Id = 11 },
            CleaningObject = new() { Id = 56 },
            StartTime = new DateTime(2022, 8, 1, 14, 00, 00),
            Bundles = new List<BundleBusinessModel> { new BundleBusinessModel { Id = 2} },
            Services = new List<Service> { new Service { Id = 42, Duration = 2, Price = 10 } },
            TotalDuration = 8,
            CleanersCount = 2,
            EndTime = new DateTime(2022, 8, 1, 18, 00, 00)
        };

        var bundle = new Bundle { Id = 2, Duration = 6, Measure = Measure.Apartment, Price = 100 };

        decimal expectedPrice = 110;
        var expectedStatus = Status.Created;

        _mockCleanersService.Setup(c => c.GetFreeCleanersForOrder(order)).Returns(cleaners);
        _mockBundlesRepository.Setup(b => b.GetBundle(2)).Returns(bundle);

        // when
        _sut.AddOrder(order);

        // then
        _mockOrdersRepository.Verify(o => o.CreateOrder(
            It.Is<Order>(
                i => i.Price == expectedPrice
                && i.Status == expectedStatus)),
            Times.Once);
        _mockCleanersService.Verify(c => c.GetFreeCleanersForOrder(order), Times.Once);
        _mockBundlesRepository.Verify(b => b.GetBundle(2), Times.Once);
    }

    [Fact]
    public void AddOrderTest_WhenCleanersIsNotEnough_ThenOrderStatusIsModeration()
    {
        // given
        var cleaners = new List<Cleaner>
        {
            new Cleaner()
            {
                Id = 11,
                Schedule = Schedule.FullTime,
                Orders = new List<Order>(),
                DateOfStartWork = new DateTime(2022, 8, 1, 00, 00, 00)
            }
        };
        var order = new OrderBusinessModel
        {
            Client = new() { Id = 11 },
            CleaningObject = new() { Id = 56 },
            StartTime = new DateTime(2022, 8, 1, 14, 00, 00),
            Bundles = new List<BundleBusinessModel> { new BundleBusinessModel { Id = 2} },
            Services = new List<Service> { new Service { Id = 42, Duration = 2, Price = 10 } },
            TotalDuration = 8,
            CleanersCount = 2,
            EndTime = new DateTime(2022, 8, 1, 18, 00, 00)
        };

        var bundle = new Bundle { Id = 2, Duration = 6, Measure = Measure.Apartment, Price = 100 };

        decimal expectedPrice = 110;
        var expectedStatus = Status.Moderation;

        _mockCleanersService.Setup(c => c.GetFreeCleanersForOrder(order)).Returns(cleaners);
        _mockBundlesRepository.Setup(b => b.GetBundle(2)).Returns(bundle);

        // when
        _sut.AddOrder(order);

        // then
        _mockOrdersRepository.Verify(o => o.CreateOrder(
            It.Is<Order>(
                i => i.Price == expectedPrice
                && i.Status == expectedStatus)),
            Times.Once);
        _mockCleanersService.Verify(c => c.GetFreeCleanersForOrder(order), Times.Once);
        _mockEmailSender.Verify(e => e.SendEmail(It.IsAny<int>()), Times.Once);
        _mockBundlesRepository.Verify(b => b.GetBundle(2), Times.Once);
    }

    [Fact]
    public void DeleteOrder_WhenValidRequestPassed_DeleteOrder()
    {
        //given
        var expectedOrder = new Order()
        {
            Id = 1,
            Client = new Client() { Id = 3 },
            IsDeleted = false
        };

        _ordersRepositoryMock.Setup(o => o.GetOrder(expectedOrder.Id)).Returns(expectedOrder);
        _ordersRepositoryMock.Setup(o => o.DeleteOrder(expectedOrder));
        userValue = new UserValues() { Email = "AdamSmith@gmail.com3", Role = Role.Admin, Id = 1 };

        //when
        _sut.DeleteOrder(expectedOrder.Id, userValue);

        //then
        _ordersRepositoryMock.Verify(c => c.DeleteOrder(expectedOrder), Times.Once);
    }

    [Fact]
    public void DeleteOrder_EmptyOrderRequest_ThrowEntityNotFoundException()
    {
        //given
        var testId = 1;
        var order = new Order();
        var testEmail = "FakeOrder@gmail.ru";
        userValue = new UserValues() { Email = testEmail, Role = Role.Admin };

        //when

        //then
        Assert.Throws<Exceptions.BadRequestException>(() => _sut.DeleteOrder(testId, userValue));
    }

    [Fact]
    public void GetAllOrders_WhenValidRequestPassed_OrdersReceived()
    {
        //given
        var orders = new List<Order>
        {
            new Order()
            {
                Id = 1,
                Client = new Client() { Id = 1 },
                CleanersBand = new List<Cleaner>() { new Cleaner(){ Id = 1}, new Cleaner(){ Id = 2} },
                CleaningObject = new CleaningObject() {Id = 1},
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Price = 20,
                Bundles = new List<Bundle>() { new Bundle(){ Id = 1}, new Bundle(){ Id = 2} },
                Services = new List<Service>() { new Service(){ Id = 1}, new Service(){ Id = 2} },
                Comments = new List<Comment>() { new Comment(){ Id = 1}, new Comment(){ Id = 2} },
                Status = Status.Created,
                IsDeleted=false
            },
            new Order()
            {
                Id = 2,
                Client = new Client() { Id = 2 },
                CleanersBand = new List<Cleaner>() { new Cleaner(){ Id = 3}, new Cleaner(){ Id = 4} },
                CleaningObject = new CleaningObject() {Id = 2},
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                Price = 30,
                IsDeleted=false
            }
        };
        _ordersRepositoryMock.Setup(o => o.GetAllOrders()).Returns(orders);

        //when
        var actual = _sut.GetAllOrders();

        //then
        Assert.NotNull(actual);
        Assert.Equal(orders.Count, actual.Count);
        _ordersRepositoryMock.Verify(c => c.GetAllOrders(), Times.Once);
    }
}