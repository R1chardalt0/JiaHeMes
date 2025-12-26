using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Application.Trace.Production.Recipes;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using Microsoft.FSharp.Core;
using Moq;
using Xunit;

namespace ChargePadLine.Tests
{
    public class BomRecipeTest : IClassFixture<MyFixture>
    {
        private readonly MyFixture _fixture;
        private readonly BomRecipeBiz _bomRecipeBiz;

        public BomRecipeTest(MyFixture fixture)
        {
            _fixture = fixture;
            _bomRecipeBiz = new BomRecipeBiz(
                _fixture.BomRecipeSpecialRepositoryMock.Object,
                _fixture.MaterialRepositoryMock.Object,
                _fixture.CtrlVsnsServiceMock.Object
            );
        }

        #region BOM主表增加测试

        [Fact]
        public async Task MapInputToCmdArgAsync_MainTable_WithValidProductCode_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_BomRecipe_AddMain("PRODUCT001", "TestBOM", "Test Description");
            var vsn = new CtrlVsn { Id = 1, ProductCode = "PRODUCT001", Current = 1000 };
            
            _fixture.CtrlVsnsServiceMock
                .Setup(x => x.TryGetVsnAsync("PRODUCT001"))
                .ReturnsAsync(vsn);

            // Act
            var result = await _bomRecipeBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal("PRODUCT001", cmdArg.ProductCode.Value);
            Assert.Equal("TestBOM", cmdArg.BomName);
            Assert.Equal("Test Description", cmdArg.Description);
            Assert.NotEmpty(cmdArg.BomCode.Value);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_MainTable_WithInvalidProductCode_ReturnsError()
        {
            // Arrange
            var input = new Input_BomRecipe_AddMain("INVALID_PRODUCT", "TestBOM", "Test Description");
            
            _fixture.CtrlVsnsServiceMock
                .Setup(x => x.TryGetVsnAsync("INVALID_PRODUCT"))
                .ReturnsAsync((CtrlVsn?)null);

            // Act
            var result = await _bomRecipeBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task AddBomRecipeAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var arg = new CmdArg_BomRecipe_AddMain("BOM001", "PRODUCT001", "TestBOM", "Test Description");
            var expectedBomRecipe = BomRecipe.MakeNew(
                bomCode: new BomCode("BOM001"),
                bomName: "TestBOM",
                prodCode: new ProductCode("PRODUCT001"),
                description: "Test Description"
            );
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<BomRecipe>(), true))
                .ReturnsAsync(expectedBomRecipe);

            // Act
            var result = await _bomRecipeBiz.AddBomRecipeAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var addedBomRecipe = result.ResultValue;
            Assert.NotNull(addedBomRecipe);
            Assert.Equal("BOM001", addedBomRecipe.BomCode.Value);
            Assert.Equal("PRODUCT001", addedBomRecipe.ProductCode.Value);
            Assert.Equal("TestBOM", addedBomRecipe.BomName);
            Assert.Equal("Test Description", addedBomRecipe.Description);
        }

        [Fact]
        public async Task AddBomRecipeAsync_WithInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_BomRecipe_AddMain("PRODUCT001", "TestBOM", "Test Description");
            var vsn = new CtrlVsn { Id = 1, ProductCode = "PRODUCT001", Current = 1000 };
            var bomRecipe = new BomRecipe { Id = 1, BomCode = new BomCode("BOM001"), ProductCode = new ProductCode("PRODUCT001"), BomName = "TestBOM", Description = "Test Description" };
            
            _fixture.CtrlVsnsServiceMock
                .Setup(x => x.TryGetVsnAsync("PRODUCT001"))
                .ReturnsAsync(vsn);

            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<BomRecipe>(), true))
                .ReturnsAsync(bomRecipe);

            // Act
            var result = await _bomRecipeBiz.AddBomRecipeAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var addedBomRecipe = result.ResultValue;
            Assert.NotNull(addedBomRecipe);
        }

        #endregion

        #region BOM子表增加测试

        [Fact]
        public async Task MapInputToCmdArgAsync_ItemTable_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_BomRecipe_AddItem(1, "BOM_ITEM_001", "MATERIAL_001", 10.5m, "Test Item Description");
            var bomRecipe = new BomRecipe { Id = 1, BomCode = new BomCode("BOM001"), ProductCode = new ProductCode("PRODUCT001"), BomName = "TestBOM" };
            var material = new Material { Id = 1, MaterialCode = "MATERIAL_001", Name = "Test Material" };
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindAsync(1))
                .ReturnsAsync(bomRecipe);

            _fixture.MaterialRepositoryMock
                .Setup(x => x.FindWithMaterialCodeAsync("MATERIAL_001"))
                .ReturnsAsync(material);

            // Act
            var result = await _bomRecipeBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var cmdArg = result.ResultValue;
            Assert.NotNull(cmdArg);
            Assert.Equal("BOM_ITEM_001", cmdArg.BomItemCode.Value);
            Assert.Equal("MATERIAL_001", cmdArg.Material.MaterialCode.Value);
            Assert.Equal(10.5m, cmdArg.Quota);
            Assert.Equal("Test Item Description", cmdArg.Description);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_ItemTable_WithInvalidBomRecipeId_ReturnsError()
        {
            // Arrange
            var input = new Input_BomRecipe_AddItem(999, "BOM_ITEM_001", "MATERIAL_001", 10.5m, "Test Item Description");
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindAsync(999))
                .ReturnsAsync((BomRecipe?)null);

            // Act
            var result = await _bomRecipeBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task MapInputToCmdArgAsync_ItemTable_WithInvalidMaterialCode_ReturnsError()
        {
            // Arrange
            var input = new Input_BomRecipe_AddItem(1, "BOM_ITEM_001", "INVALID_MATERIAL", 10.5m, "Test Item Description");
            var bomRecipe = new BomRecipe { Id = 1, BomCode = "BOM001", ProductCode = "PRODUCT001", BomName = "TestBOM" };
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindAsync(1))
                .ReturnsAsync(bomRecipe);

            _fixture.MaterialRepositoryMock
                .Setup(x => x.FindWithMaterialCodeAsync("INVALID_MATERIAL"))
                .ReturnsAsync((Material?)null);

            // Act
            var result = await _bomRecipeBiz.MapInputToCmdArgAsync(input);

            // Assert
            Assert.True(result.IsError);
            var error = result.ErrorValue;
            Assert.NotNull(error);
        }

        [Fact]
        public async Task AddBomRecipeItemAsync_WithValidArg_ReturnsSuccess()
        {
            // Arrange
            var bomRecipe = new BomRecipe { Id = 1, BomCode = new BomCode("BOM001"), ProductCode = new ProductCode("PRODUCT001"), BomName = "TestBOM" };
            var material = new Material { Id = 1, MaterialCode = new MaterialCode("MATERIAL_001"), Name = "Test Material" };
            var arg = new CmdArg_BomRecipe_AddItem(bomRecipe, new BomItemCode("BOM_ITEM_001"), material, 10.5m, "Test Item Description");
            var bomRecipeItem = new BomRecipeItem { Id = 1, BomId = 1, BomItemCode = new BomItemCode("BOM_ITEM_001"), MaterialCode = new MaterialCode("MATERIAL_001"), Quota = 10.5m };
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1); // 返回受影响的行数

            // Act
            var result = await _bomRecipeBiz.AddBomRecipeItemAsync(arg);

            // Assert
            Assert.True(result.IsOk);
            var addedItem = result.ResultValue;
            Assert.NotNull(addedItem);
        }

        [Fact]
        public async Task AddBomRecipeItemAsync_WithInput_ReturnsSuccess()
        {
            // Arrange
            var input = new Input_BomRecipe_AddItem(1, "BOM_ITEM_001", "MATERIAL_001", 10.5m, "Test Item Description");
            var bomRecipe = new BomRecipe { Id = 1, BomCode = new BomCode("BOM001"), ProductCode = new ProductCode("PRODUCT001"), BomName = "TestBOM" };
            var material = new Material { Id = 1, MaterialCode = new MaterialCode("MATERIAL_001"), Name = "Test Material" };
            var bomRecipeItem = new BomRecipeItem { Id = 1, BomId = 1, BomItemCode = new BomItemCode("BOM_ITEM_001"), MaterialCode = new MaterialCode("MATERIAL_001"), Quota = 10.5m };
            
            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.FindAsync(1))
                .ReturnsAsync(bomRecipe);

            _fixture.MaterialRepositoryMock
                .Setup(x => x.FindWithMaterialCodeAsync("MATERIAL_001"))
                .ReturnsAsync(material);

            _fixture.BomRecipeSpecialRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _bomRecipeBiz.AddBomRecipeItemAsync(input);

            // Assert
            Assert.True(result.IsOk);
            var addedItem = result.ResultValue;
            Assert.NotNull(addedItem);
        }

        #endregion
    }
}