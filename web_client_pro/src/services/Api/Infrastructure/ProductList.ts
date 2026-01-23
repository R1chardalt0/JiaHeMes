import { request } from '@umijs/max';
import { ProductListDto, ProductListQueryDto } from '@/services/Model/Infrastructure/ProductList';

/**
 * 分页查询产品列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getProductListList(
  params: ProductListQueryDto
): Promise<any> {
  const result = await request('/api/ProductList/GetProductListList', {
    method: 'GET',
    params,
  });

  // 处理后端返回的数据格式
  // 如果返回的是数组，转换为分页格式
  if (Array.isArray(result)) {
    return {
      data: result,
      total: result.length,
      success: true
    };
  }

  // 如果已经是分页格式，直接返回
  return result;
}

/**
 * 根据ID获取产品详情
 * @param id 产品ID
 * @returns 产品详情
 */
export async function getProductListById(
  id: string
): Promise<ProductListDto> {
  return request('/api/ProductList/GetProductListById', {
    method: 'GET',
    params: { id },
  });
}

/**
 * 创建产品
 * @param data 创建产品DTO
 * @returns 创建成功的产品信息
 */
export async function createProductList(
  data: ProductListDto
): Promise<ProductListDto> {
  return request('/api/ProductList/CreateProductList', {
    method: 'POST',
    data,
  });
}

/**
 * 更新产品
 * @param id 产品ID
 * @param data 更新产品DTO
 * @returns 更新后的产品信息
 */
export async function updateProductList(
  id: string,
  data: ProductListDto
): Promise<ProductListDto> {
  return request('/api/ProductList/UpdateProductListById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除产品
 * @param ids 产品ID或ID数组
 * @returns 删除结果
 */
export async function deleteProductList(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/ProductList/DeleteProductListByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}