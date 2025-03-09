using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.Core.Abstractions.Repositories;
using System.Threading.Tasks;
using System;
using Xunit;
using AutoFixture;
using PromoCodeFactory.Core.Services;
using PromoCodefactory.UnitTests.Builders;
using FluentAssertions;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IPartnerRepository> _mockRepository;
        private readonly PartnerLimitService _partnerLimitService;
        private readonly PartnersController _controller;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            _fixture = new Fixture();
            _mockRepository = new Mock<IPartnerRepository>();
            _partnerLimitService = new PartnerLimitService(_mockRepository.Object);
            _controller = new PartnersController(_mockRepository.Object, _partnerLimitService);
        }

        /// <summary>
        /// Если партнер не найден, то также нужно выдать ошибку 404;
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var request = PartnerTestDataBuilder.CreateRequest();

            _mockRepository.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync((Partner)null);

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        /// <summary>
        /// Если партнер заблокирован (IsActive = false), то возвращается ошибка 400
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerInactive_ReturnsBadRequest()
        {
            // Arrange
            var partner = PartnerTestDataBuilder.CreatePartner(isActive: false); // Партнер заблокирован
            var request = PartnerTestDataBuilder.CreateRequest();

            _mockRepository.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Данный партнер не активен");
        }

        /// <summary>
        /// Если партнеру выставляется лимит, количество выданных промокодов (NumberIssuedPromoCodes) обнуляется, если лимит закончился
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_NewLimitSet_ResetsNumberIssuedPromoCodes()
        {
            // Arrange
            var partner = PartnerTestDataBuilder.CreatePartner();
            partner.NumberIssuedPromoCodes = 10; // Устанавливаем начальное значение

            // Добавляем активный лимит
            var activeLimit = new PartnerPromoCodeLimit
            {
                Id = Guid.NewGuid(),
                CancelDate = null, // Активный лимит
                CreateDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(10),
                Limit = 100
            };
            partner.PartnerLimits.Add(activeLimit);

            var request = PartnerTestDataBuilder.CreateRequest();

            _mockRepository.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Используем Callback для обновления состояния partner
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Partner>()))
                .Returns(Task.CompletedTask)
                .Callback<Partner>(p => partner = p); // Обновляем состояние partner

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();

            // Проверяем, что NumberIssuedPromoCodes обнулилось
            partner.NumberIssuedPromoCodes.Should().Be(0);

            // Проверяем, что UpdateAsync был вызван
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Partner>()), Times.Once);
        }

        /// <summary>
        /// При установке лимита нужно отключить предыдущий лимит
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ActiveLimitExists_DeactivatesOldLimit()
        {
            // Arrange
            var partner = PartnerTestDataBuilder.CreatePartner();
            var activeLimit = new PartnerPromoCodeLimit
            {
                Id = Guid.NewGuid(),
                CancelDate = null, // Активный лимит
                CreateDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(10),
                Limit = 100
            };
            partner.PartnerLimits.Add(activeLimit);

            var request = PartnerTestDataBuilder.CreateRequest();

            _mockRepository.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Partner>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();

            // Проверяем, что старый лимит отключен
            activeLimit.CancelDate.Should().NotBeNull();

            // Проверяем, что новый лимит добавлен
            partner.PartnerLimits.Should().HaveCount(2);
        }

        /// <summary>
        ///  Лимит должен быть больше 0
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_LimitLessOrEqualToZero_ReturnsBadRequest()
        {
            // Arrange
            var partner = PartnerTestDataBuilder.CreatePartner();
            var request = PartnerTestDataBuilder.CreateRequest(limit: 0); // Лимит <= 0

            _mockRepository.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Лимит должен быть больше 0");
        }

        /// <summary>
        /// Убедиться, что новый лимит сохраняется в базу данных
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_NewLimitSavedToDatabase()
        {
            // Arrange
            var partner = PartnerTestDataBuilder.CreatePartner();
            var request = PartnerTestDataBuilder.CreateRequest();

            _mockRepository.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Partner>()))
                .Returns(Task.CompletedTask)
                .Callback<Partner>(p => partner = p); // Сохраняем обновленного партнера

            // Act
            var result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();

            // Проверяем, что новый лимит добавлен в партнера
            partner.PartnerLimits.Should().ContainSingle(limit =>
                limit.Limit == request.Limit &&
                limit.EndDate == request.EndDate);

            // Проверяем, что репозиторий был вызван для обновления партнера
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Partner>()), Times.Once);
        }



    }
}