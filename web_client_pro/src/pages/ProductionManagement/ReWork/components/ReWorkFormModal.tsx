import { useImperativeHandle, forwardRef, useState, useEffect } from 'react';
import { Modal, Form, Input, Button, Space, message, Select } from 'antd';
import type { FormInstance } from 'antd';
import { ReWorkFormData } from '@/services/Model/Production/reWork';
import { getMesSnListCurrentBySnNumber } from '@/services/Api/Trace/MesSnListCurrent';
import { getStationListById } from '@/services/Api/Infrastructure/StationList';
import { getMESOrderBomBatchItemBySnNumber } from '@/services/Api/Infrastructure/MESOrderBomBatch/MESOrderBomBatchItem';
import { getMESOrderBomBatchById } from '@/services/Api/Infrastructure/MESOrderBomBatch/MESOrderBomBatch';
import { getProductListById } from '@/services/Api/Infrastructure/ProductList';

// 返工表单模态框属性接口
export interface ReWorkFormModalProps {
  // 模态框是否可见
  open: boolean;
  // 模态框标题
  title: string;
  // 初始表单值
  initialValues?: ReWorkFormData;
  // 取消回调
  onCancel: () => void;
  // 保存回调
  onSave: (values: ReWorkFormData, callback: (success: boolean) => void) => void;
}

// 返工表单模态框引用接口
export interface ReWorkFormModalRef {
  // 获取表单实例
  getForm: () => FormInstance<ReWorkFormData>;
}

/**
 * 返工表单模态框组件
 * 用于SN返工操作
 */
const ReWorkFormModal = forwardRef<ReWorkFormModalRef, ReWorkFormModalProps>(({
  title,
  initialValues,
  onCancel,
  onSave
}, ref) => {
  // 创建表单实例
  const [form] = Form.useForm<ReWorkFormData>();
  // 当前SN码
  const [currentSN, setCurrentSN] = useState<string>('');
  // 当前站点
  const [currentStation, setCurrentStation] = useState<string>('');
  // 加载状态
  const [loading, setLoading] = useState<boolean>(false);
  // 是否显示当前站点信息
  const [showCurrentStation, setShowCurrentStation] = useState<boolean>(false);
  // 站点列表
  const [stations, setStations] = useState<Array<{ value: string; label: string }>>([]);
  // 是否显示目标站点下拉框
  const [showTargetStation, setShowTargetStation] = useState<boolean>(false);
  // 物料信息列表（包含解绑状态）
  const [materialList, setMaterialList] = useState<Array<{
    productListId: string;
    productName: string;
    orderBomBatchItemId: string;
    isUnbind: boolean;
  }>>([]);
  // 物料查询加载状态
  const [materialLoading, setMaterialLoading] = useState<boolean>(false);
  // 是否显示物料信息区域
  const [showMaterialInfo, setShowMaterialInfo] = useState<boolean>(false);

  // 暴露form实例给父组件
  useImperativeHandle(ref, () => ({
    getForm: () => form
  }));

  // 生成站点列表（OP10到OP170）
  const generateStationList = () => {
    const stationList = [];
    for (let i = 10; i <= 170; i += 10) {
      const stationCode = `OP${i}`;
      stationList.push({ value: stationCode, label: stationCode });
    }
    return stationList;
  };

  // 从站点编码中提取数字部分（如 "OP30" -> 30）
  const getStationNumber = (stationCode: string): number => {
    if (!stationCode) return 0;
    const match = stationCode.match(/\d+/);
    return match ? parseInt(match[0], 10) : 0;
  };

  // 监听SN码变化
  const handleSNChange = async (value: string) => {
    setCurrentSN(value);

    if (!value) {
      setShowTargetStation(false);
      setStations([]);
      setCurrentStation('');
      setShowCurrentStation(false);
      form.setFieldsValue({ targetStationId: '' });
      // 重置物料相关状态
      setMaterialList([]);
      setMaterialLoading(false);
      setShowMaterialInfo(false);
      return;
    }

    try {
      setLoading(true);

      // 1. 调用真实API获取SN当前状态
      const snInfo = await getMesSnListCurrentBySnNumber(value);
      console.log('SN信息API返回结果:', snInfo);

      // 检查API返回结果
      if (!snInfo) {
        message.error('未找到SN信息');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 兼容不同可能的返回结构
      const currentStationListId = snInfo.currentStationListId;
      if (!currentStationListId) {
        message.error('未找到SN对应的当前站点信息');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 2. 从SN信息中获取当前站点ID
      const currentStationId = currentStationListId;
      console.log('当前站点ID:', currentStationId);

      // 3. 调用API根据站点ID获取站点详情
      const stationInfo = await getStationListById(currentStationId);
      console.log('站点信息API返回结果:', stationInfo);

      // 检查站点信息
      if (!stationInfo) {
        message.error('未找到当前站点详情');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 兼容不同可能的返回结构
      const stationCode = stationInfo.stationCode;
      if (!stationCode) {
        message.error('未找到当前站点编码');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 4. 获取当前站点编码
      const currentStationCode = stationCode;
      console.log('当前站点编码:', currentStationCode);

      // 设置当前站点
      setCurrentStation(currentStationCode);
      setShowCurrentStation(true);

      // 5. 生成所有站点
      const allStations = generateStationList();
      console.log('所有站点列表:', allStations);

      // 6. 找到当前站点的索引
      const currentIndex = allStations.findIndex(s => s.value === currentStationCode);
      console.log('当前站点索引:', currentIndex);

      if (currentIndex === -1) {
        message.error('当前站点不在预设站点列表中');
        setShowTargetStation(false);
        setStations([]);
        return;
      }

      // 7. 只显示当前站点之前的站点（返工操作只能选择之前的站点）
      const availableStations = allStations.slice(0, currentIndex);
      console.log('可用目标站点列表:', availableStations);
      setStations(availableStations);
      setShowTargetStation(true);
      form.setFieldsValue({ targetStationId: '' });

    } catch (error) {
      message.error('获取SN当前状态失败，请稍后重试');
      console.error('获取SN当前状态失败:', error);
      // 即使API调用失败，也显示目标站点选择框（使用所有站点）
      const allStations = generateStationList();
      setStations(allStations);
      setShowTargetStation(true);
      setCurrentStation('');
      setShowCurrentStation(false);
    } finally {
      setLoading(false);
    }
  };

  // 处理取消
  const handleCancel = () => {
    form.resetFields();
    setCurrentSN('');
    setCurrentStation('');
    setShowCurrentStation(false);
    setShowTargetStation(false);
    setStations([]);
    // 重置物料相关状态
    setMaterialList([]);
    setMaterialLoading(false);
    setShowMaterialInfo(false);
    onCancel();
  };

  // 处理目标站点选择变化
  const handleTargetStationChange = async (value: string) => {
    // 如果没有选择站点或没有SN码，不执行操作
    if (!value || !currentSN) {
      return;
    }

    try {
      setMaterialLoading(true);
      setMaterialList([]);

      // 1. 通过SN号获取所有MESOrderBomBatchItem
      const bomBatchItemResult = await getMESOrderBomBatchItemBySnNumber(currentSN);

      // 处理可能返回单个对象或数组的情况
      const bomBatchItems = Array.isArray(bomBatchItemResult) ? bomBatchItemResult : [bomBatchItemResult];

      // 过滤掉已解绑的物料
      const boundBomBatchItems = bomBatchItems.filter(item => item.isUnbind !== true);

      if (!boundBomBatchItems || boundBomBatchItems.length === 0) {
        message.error('未找到绑定的物料信息');
        return;
      }

      // 2. 获取所有唯一的orderBomBatchId，包括重复的，确保每个batchItem都能找到对应的batch信息
      const allOrderBomBatchIds = boundBomBatchItems.map(item => item.orderBomBatchId).filter(id => id);

      if (allOrderBomBatchIds.length === 0) {
        message.error('未找到物料批次信息');
        return;
      }

      // 3. 并行获取所有MESOrderBomBatch信息（去重，避免重复查询）
      const uniqueOrderBomBatchIds = Array.from(new Set(allOrderBomBatchIds));
      const bomBatchPromises = uniqueOrderBomBatchIds.map(id => getMESOrderBomBatchById(id));
      const bomBatches = await Promise.all(bomBatchPromises);

      // 4. 获取所有需要的站点ID并去重（用于后续获取站点编码）
      const stationListIds = bomBatches
        .filter(batch => batch && batch.stationListId)
        .map(batch => batch.stationListId!);
      const uniqueStationListIds = Array.from(new Set(stationListIds));

      // 并行获取所有站点信息
      const stationPromises = uniqueStationListIds.map(id => getStationListById(id));
      const stationsInfo = await Promise.all(stationPromises);

      // 构建站点映射表（stationId -> stationCode）
      const stationMap = new Map<string, string>();
      stationsInfo
        .filter(station => station && station.stationCode)
        .forEach(station => stationMap.set(station.stationId, station.stationCode));

      // 过滤有效的批次信息并构建映射表，同时补充stationCode字段
      const batchMap = new Map<string, any>();
      bomBatches
        .filter(batch => batch && batch.productListId)
        .forEach(batch => {
          // 如果batch中有stationListId但没有stationCode，则从stationMap中获取
          if (batch.stationListId && !batch.stationCode) {
            batch.stationCode = stationMap.get(batch.stationListId) || '未知站点';
          }
          // 如果batch既没有stationCode也没有stationListId，设置默认值
          if (!batch.stationCode) {
            batch.stationCode = '未知站点';
          }
          batchMap.set(batch.orderBomBatchId, batch);
        });

      if (batchMap.size === 0) {
        message.error('未找到有效物料批次信息');
        return;
      }

      // 5. 获取所有需要的产品ID并去重
      const allProductIds = Array.from(batchMap.values()).map(batch => batch.productListId!);
      const uniqueProductIds = Array.from(new Set(allProductIds));

      // 并行获取所有productName
      const productPromises = uniqueProductIds.map(id => getProductListById(id));
      const products = await Promise.all(productPromises);

      // 构建产品映射表
      const productMap = new Map<string, any>();
      products.forEach(product => productMap.set(product.productListId, product));

      // 6. 获取当前站点和目标站点的数字值，用于筛选物料
      const currentStationNum = getStationNumber(currentStation);
      const targetStationNum = getStationNumber(value);

      // 确定站点范围的起始和结束值
      const startStation = Math.min(currentStationNum, targetStationNum);
      const endStation = Math.max(currentStationNum, targetStationNum);

      // 7. 构建物料列表（包含orderBomBatchItemId和初始解绑状态），并只保留当前站点和目标站点之间的物料
      const materialData = boundBomBatchItems.map((batchItem) => {
        // 找到对应的batch信息
        const batch = batchMap.get(batchItem.orderBomBatchId);
        if (!batch) return null;

        // 获取该物料批次的站点编码，并提取数字部分
        const batchStationCode = batch.stationCode;
        const batchStationNum = getStationNumber(batchStationCode);

        // 只保留在当前站点和目标站点之间的物料
        if (batchStationNum < startStation || batchStationNum > endStation) {
          return null;
        }

        // 找到对应的产品信息
        const product = productMap.get(batch.productListId);

        return {
          productListId: batch.productListId!,
          productName: product?.productName || '未知物料',
          orderBomBatchItemId: batchItem.orderBomBatchItemId || '',
          isUnbind: false // 默认不解绑
        };
      }).filter(material => material !== null);

      if (materialData.length === 0) {
        message.error('当前站点和目标站点之间未找到有效物料信息');
        return;
      }

      setMaterialList(materialData);
      setShowMaterialInfo(true);

    } catch (error) {
      message.error('获取物料信息失败，请稍后重试');
      console.error('获取物料信息失败:', error);
    } finally {
      setMaterialLoading(false);
    }
  };

  // 切换物料解绑状态
  const handleToggleUnbind = (orderBomBatchItemId: string) => {
    setMaterialList(prevList =>
      prevList.map(item =>
        item.orderBomBatchItemId === orderBomBatchItemId
          ? { ...item, isUnbind: !item.isUnbind }
          : item
      )
    );
  };

  // 处理保存
  const handleSave = async () => {
    try {
      const values = await form.validateFields();

      // 扩展表单值，添加物料解绑信息
      const submitValues = {
        ...values,
        // 传递需要解绑的物料批次明细ID列表
        UnbindMaterialIds: materialList
          .filter(item => item.isUnbind && item.orderBomBatchItemId)
          .map(item => item.orderBomBatchItemId)
      };

      // 调用保存回调，等待其完成
      const saveResult = await new Promise((resolve) => {
        // 传递一个回调函数给onSave，让它通知我们结果
        onSave(submitValues, (success: boolean) => {
          resolve(success);
        });
      });

      // 只有在保存成功时才重置表单和状态
      if (saveResult === true) {
        form.resetFields();
        setCurrentSN('');
        setCurrentStation('');
        setShowCurrentStation(false);
        setShowTargetStation(false);
        setStations([]);
        // 重置物料相关状态
        setMaterialList([]);
        setMaterialLoading(false);
        setShowMaterialInfo(false);
      }
    } catch (error) {
      // 表单验证失败或其他错误，不重置表单
      console.error('保存失败:', error);
    }
  };

  return (
    <>
      <div style={{ maxWidth: 600, margin: '0 auto' }}>
        <h2>{title}</h2>
        <Form
          form={form}
          layout="vertical"
          autoComplete="off"
          initialValues={initialValues}
          onKeyDown={(e) => {
            // 阻止Enter键的默认提交行为
            if (e.key === 'Enter') {
              e.preventDefault();
            }
          }}
        >
          <Form.Item
            label="SN码"
            name="sn"
            rules={[
              { required: true, message: '请输入SN码' },
            ]}
          >
            <Input
              placeholder="请输入SN码"
              onPressEnter={(e) => {
                handleSNChange((e.target as HTMLInputElement).value);
              }}
            />
          </Form.Item>

          <Form.Item
            label="当前站点编码"
          >
            <Input
              value={currentStation}
              readOnly
              style={{ backgroundColor: '#f5f5f5' }}
            />
          </Form.Item>

          <Form.Item
            label="目标站点编码"
            name="targetStationId"
            rules={[
              { required: true, message: '请选择目标站点编码' },
            ]}
          >
            <Select
              placeholder="请选择目标站点编码"
              options={stations}
              loading={loading}
              style={{ width: '100%' }}
              onChange={handleTargetStationChange}
            />
          </Form.Item>

          {/* 已绑定物料信息列表 */}
          {showMaterialInfo && (
            <>
              <Form.Item label="已绑定物料信息">
                <div style={{ border: '1px solid #e8e8e8', borderRadius: '4px', padding: '8px' }}>
                  {materialLoading ? (
                    <div style={{ textAlign: 'center', padding: '20px 0' }}>加载中...</div>
                  ) : materialList.length === 0 ? (
                    <div style={{ textAlign: 'center', padding: '20px 0' }}>未找到绑定的物料信息</div>
                  ) : (
                    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                      <thead>
                        <tr style={{ borderBottom: '1px solid #e8e8e8' }}>
                          <th style={{ padding: '8px 16px', textAlign: 'left' }}>物料名称</th>
                          <th style={{ padding: '8px 16px', textAlign: 'center' }}>操作</th>
                        </tr>
                      </thead>
                      <tbody>
                        {materialList.map((material) => (
                          <tr key={material.orderBomBatchItemId} style={{ borderBottom: '1px solid #e8e8e8' }}>
                            <td style={{ padding: '8px 16px' }}>{material.productName}</td>
                            <td style={{ padding: '8px 16px', textAlign: 'center' }}>
                              <Button
                                type={material.isUnbind ? 'primary' : 'default'}
                                size="small"
                                onClick={() => handleToggleUnbind(material.orderBomBatchItemId)}
                              >
                                {material.isUnbind ? '已解绑' : '解绑'}
                              </Button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  )}
                </div>
              </Form.Item>

              {/* 解绑提示信息 */}
              <div style={{ marginBottom: '16px', color: '#666' }}>
                提示：点击每条物料后的"解绑"按钮可单独解绑对应物料
              </div>
            </>
          )}

          <Form.Item>
            <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
              <Button onClick={handleCancel}>重置</Button>
              <Button type="primary" onClick={handleSave}>保存</Button>
            </div>
          </Form.Item>
        </Form>
      </div>


    </>
  );
});

ReWorkFormModal.displayName = 'ReWorkFormModal';

export default ReWorkFormModal;