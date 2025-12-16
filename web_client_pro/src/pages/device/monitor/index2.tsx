import { Card, List, Typography } from 'antd';
import type { DeviceDetailType } from './data.d';
import { getDeviceDetail } from './service';
import useStyles from './style.style';
import { useRequest, useNavigate } from '@umijs/max';
import React, { useState, useEffect, useMemo } from 'react';
import { PageContainer } from '@ant-design/pro-components';

const { Title } = Typography;

const DeviceList2 = () => {
  const { styles } = useStyles();
  const navigate = useNavigate();

  // 获取设备数据
  const { data: apiResponse, loading } = useRequest(() => getDeviceDetail(), {
    formatResult: (res) => res,
  });

  const [list, setList] = useState<DeviceDetailType[]>([]);

  useEffect(() => {
    if (apiResponse && typeof apiResponse === 'object') {
      const isSuccess = apiResponse.success === true;
      if (isSuccess) {
        if ('data' in apiResponse && Array.isArray(apiResponse.data)) {
          // 调试：查看原始数据
          console.log('原始设备数据:', apiResponse.data);
          if (apiResponse.data.length > 0) {
            console.log('第一条设备数据示例:', apiResponse.data[0]);
            console.log('字段名:', Object.keys(apiResponse.data[0]));
          }
          setList(apiResponse.data);
        }
      }
    }
  }, [apiResponse]);

  // 根据设备类型动态去重，并获取每个类型的第一条数据的头像
  const deviceTypeItems = useMemo(() => {
    const typeMap = new Map<string, { count: number; firstDevice: DeviceDetailType | null }>();

    list.forEach((device) => {
      const normalizedType = (device.deviceType ?? '').trim();
      const typeLabel = normalizedType !== '' ? normalizedType : '未分类';
      
      if (!typeMap.has(typeLabel)) {
        typeMap.set(typeLabel, { count: 0, firstDevice: device });
      }
      const typeInfo = typeMap.get(typeLabel)!;
      typeInfo.count++;
    });

    return Array.from(typeMap.entries())
      .map(([label, info]) => {
        if (!info.firstDevice) {
          return {
            key: label,
            label,
            count: info.count,
            avatar: '',
          };
        }

        // 兼容两种命名方式：PascalCase (后端) 和 camelCase (前端)
        const device = info.firstDevice as any;
        // 头像仅使用 Avatar 字段，不再混用 DevicePicture
        const avatar = device.avatar || device.Avatar || '';
        let imageUrl = avatar || '';
        
        if (!imageUrl) {
          return {
            key: label,
            label,
            count: info.count,
            avatar: '',
          };
        }
        
        // 处理转义的反斜杠（将 \\ 转换为 \）
        imageUrl = imageUrl.replace(/\\\\/g, '\\');
        
        // 检查是否是完整的 URL（http://、https:// 或 / 开头）
        const isFullUrl = imageUrl && (
          imageUrl.startsWith('http://') || 
          imageUrl.startsWith('https://') || 
          (imageUrl.startsWith('/') && imageUrl.length > 1 && !imageUrl.startsWith('/images/'))
        );
        
        if (isFullUrl) {
          // 已经是完整的 URL，直接使用
          console.log(`设备类型 ${label}: 使用完整 URL`, imageUrl);
        } else {
          // 检查是否是本地文件路径（Windows 路径格式，如 D:\ 或 D:/）
          const isLocalPath = imageUrl && (
            /^[A-Za-z]:[\\/]/.test(imageUrl) || // Windows 绝对路径 D:\ 或 D:/
            imageUrl.startsWith('\\') || // Windows 网络路径 \\server\share
            imageUrl.startsWith('file://') // file:// 协议
          );
          
          if (isLocalPath) {
            // 从本地路径提取文件名
            try {
              const normalizedPath = imageUrl.replace(/\\/g, '/');
              const pathParts = normalizedPath.split('/');
              const fileName = pathParts[pathParts.length - 1] || '';
              
              if (fileName && fileName.includes('.')) {
                // 使用 /images/ 路径，因为图片在 public/images/ 文件夹中
                // 直接使用文件名，浏览器会自动处理中文编码
                imageUrl = `/images/${fileName}`;
                console.log(`设备类型 ${label}: 将本地路径转换为服务器路径`, {
                  原始路径: avatar,
                  文件名: fileName,
                  转换后路径: imageUrl,
                });
              } else {
                console.warn(`设备类型 ${label}: 无法提取有效文件名`, {
                  原始路径: avatar,
                });
                imageUrl = '';
              }
            } catch (e) {
              console.error(`设备类型 ${label}: 路径转换出错`, e, {
                原始路径: avatar,
              });
              imageUrl = '';
            }
          } else {
            // 既不是完整 URL，也不是本地路径，可能是纯文件名（如 "催化炉.png"）
            // 检查是否包含路径分隔符
            const hasPathSeparator = imageUrl.includes('/') || imageUrl.includes('\\');
            
            if (!hasPathSeparator && imageUrl.includes('.')) {
              // 纯文件名，使用 /images/ 路径（图片在 public/images/ 文件夹中）
              // 直接使用文件名，浏览器会自动处理中文编码
              imageUrl = `/images/${imageUrl}`;
              console.log(`设备类型 ${label}: 将纯文件名转换为服务器路径`, {
                原始文件名: avatar,
                转换后路径: imageUrl,
              });
            } else if (imageUrl.startsWith('/images/')) {
              // 已经是 /images/ 开头的路径，直接使用
              console.log(`设备类型 ${label}: 使用 /images/ 路径`, imageUrl);
            } else {
              // 无法识别的格式，尝试提取文件名
              try {
                const normalizedPath = imageUrl.replace(/\\/g, '/');
                const pathParts = normalizedPath.split('/');
                const fileName = pathParts[pathParts.length - 1] || '';
                if (fileName && fileName.includes('.')) {
                  // 直接使用文件名，浏览器会自动处理中文编码
                  imageUrl = `/images/${fileName}`;
                  console.log(`设备类型 ${label}: 从路径中提取文件名`, {
                    原始路径: avatar,
                    文件名: fileName,
                    转换后路径: imageUrl,
                  });
                } else {
                  console.warn(`设备类型 ${label}: 无法识别的图片路径格式`, {
                    原始路径: avatar,
                    处理后的路径: imageUrl,
                  });
                  imageUrl = '';
                }
              } catch (e) {
                console.error(`设备类型 ${label}: 路径处理出错`, e, {
                  原始路径: avatar,
                });
                imageUrl = '';
              }
            }
          }
        }
        
        // 调试信息
        console.log(`设备类型 ${label}:`, {
          avatar_camel: device.avatar,
          avatar_Pascal: device.Avatar,
          finalImageUrl: imageUrl,
        });
        
        return {
          key: label,
          label,
          count: info.count,
          avatar: imageUrl,
        };
      })
      .sort((a, b) => a.label.localeCompare(b.label, 'zh-CN'));
  }, [list]);

  return (
    <PageContainer>
      <div className={styles.cardList}>
        <List
          rowKey="key"
          loading={loading}
          grid={{
            gutter: 16,
            xs: 1,
            sm: 2,
            md: 3,
            lg: 3,
            xl: 3,
            xxl: 4,
          }}
          dataSource={deviceTypeItems}
          locale={{ emptyText: '暂无设备类型数据' }}
          renderItem={(item) => {
            // 将头像改为矩形显示
            const avatarRectStyle: React.CSSProperties = {
              cursor: 'pointer',
              width: '390px',
              height: '230px',
              // marginRight: '-16px',
              margin: '-40px -13px 0 0',
              objectFit: 'cover',
              // marginTop:'50px',
              // objectPosition: 'center',
              borderRadius: 0,
              // transition: 'transform 0.18s ease, filter 0.18s ease',
              display: 'block',              
            };
             return (
               <List.Item key={item.key}>
                 <Card
                   hoverable
                   className={`${styles.card} ${styles.typeCard}`}
                   actions={[
                     <a
                       key="monitor"
                       onClick={(e) => {
                         e.preventDefault();
                         // 跳转到监控页面，传递设备类型参数
                         navigate(`/devicechart/monitor/${encodeURIComponent(item.label)}`);
                       }}
                     >
                       监控数据
                     </a>,
                   ]}
                 >
                   <Card.Meta
                     avatar={
                      item.avatar ? (
                        <img
                          alt={item.label}
                          className={styles.cardAvatar}
                          src={item.avatar}
                          style={{ ...avatarRectStyle, objectFit: 'cover' }}
                          onClick={() => {
                            navigate(`/devicechart/monitor/${encodeURIComponent(item.label)}`);
                          }}
                          onMouseEnter={(e) => {
                            (e.currentTarget as HTMLImageElement).style.transform = 'scale(1.03)';
                          }}
                          onMouseLeave={(e) => {
                            (e.currentTarget as HTMLImageElement).style.transform = 'scale(1)';
                          }}
                          onError={(e) => {
                            console.error(`设备类型 ${item.label} 图片加载失败:`, {
                              url: item.avatar,
                              error: e,
                            });
                            const target = e.target as HTMLImageElement;
                            target.style.display = 'none';
                            const parent = target.parentElement;
                            if (parent) {
                              let placeholder = parent.querySelector('.avatar-placeholder') as HTMLElement;
                              if (!placeholder) {
                                placeholder = document.createElement('span');
                                placeholder.className = `${styles.cardAvatarPlaceholder} avatar-placeholder`;
                                Object.assign(placeholder.style, {
                                  cursor: 'pointer',
                                  display: 'block',
                                  width: avatarRectStyle.width,
                                  height: `${avatarRectStyle.height}px`,
                                  borderRadius: `${avatarRectStyle.borderRadius}px`,
                                  background: '#f5f5f5',
                                });
                                placeholder.onclick = () => {
                                  navigate(`/devicechart/monitor/${encodeURIComponent(item.label)}`);
                                };
                                parent.insertBefore(placeholder, target);
                              }
                              placeholder.style.display = 'inline-block';
                            }
                          }}
                          onLoad={(e) => {
                            const target = e.target as HTMLImageElement;
                            const parent = target.parentElement;
                            if (parent) {
                              const placeholder = parent.querySelector('.avatar-placeholder') as HTMLElement;
                              if (placeholder) {
                                placeholder.style.display = 'none';
                              }
                            }
                          }}
                        />
                      ) : (
                        <span
                          className={styles.cardAvatarPlaceholder}
                          style={{
                            cursor: 'pointer',
                            display: 'block',
                            width: avatarRectStyle.width,
                            height: `${avatarRectStyle.height}px`,
                            borderRadius: `${avatarRectStyle.borderRadius}px`,
                            background: '#f5f5f5',
                          }}
                          onClick={() => {
                            navigate(`/devicechart/monitor/${encodeURIComponent(item.label)}`);
                          }}
                        />
                      )
                     }
                     title={
                       <a 
                         onClick={(e) => {
                           e.preventDefault();
                           navigate(`/devicechart/monitor/${encodeURIComponent(item.label)}`);
                         }}
                         style={{ cursor: 'pointer' }}
                       >
                         {item.label}
                       </a>
                     }
                     description={<div className={styles.item} />}
                   />
                 </Card>
               </List.Item>
             );
           }}
        />
      </div>
    </PageContainer>
  );
};

export default DeviceList2;

