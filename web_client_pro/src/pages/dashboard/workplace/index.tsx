import { Radar } from '@ant-design/plots';
import { PageContainer } from '@ant-design/pro-components';
import { Link, useRequest } from '@umijs/max';
import { Avatar, Card, Col, List, Row, Skeleton, Statistic } from 'antd';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import type { FC } from 'react';
import EditableLinkGroup from './components/EditableLinkGroup';
import type { ActivitiesType, CurrentUser } from './data.d';
import { fakeChartData, queryActivities, queryProjectNotice } from './service';
import useStyles from './style.style';

dayjs.extend(relativeTime);

const links = [
  {
    title: '操作一',
    href: '',
  },
  {
    title: '操作二',
    href: '',
  },
  {
    title: '操作三',
    href: '',
  },
  {
    title: '操作四',
    href: '',
  },
  {
    title: '操作五',
    href: '',
  },
  {
    title: '操作六',
    href: '',
  },
];
const PageHeaderContent: FC<{
  currentUser: Partial<CurrentUser>;
}> = ({ currentUser }) => {
  const { styles } = useStyles();
  const loading = currentUser && Object.keys(currentUser).length;
  if (!loading) {
    return (
      <Skeleton
        avatar
        paragraph={{
          rows: 1,
        }}
        active
      />
    );
  }
  return (
    <div className={styles.pageHeaderContent}>
      <div className={styles.avatar}>
        <Avatar size="large" src={currentUser.avatar} />
      </div>
      <div className={styles.content}>
        <div className={styles.contentTitle}>
          早安，
          {currentUser.name}
          ，祝你开心每一天！
        </div>
        <div>
          {currentUser.title} |{currentUser.group}
        </div>
      </div>
    </div>
  );
};
const ExtraContent: FC<Record<string, any>> = () => {
  const { styles } = useStyles();
  return (
    <div className={styles.extraContent}>
      <div className={styles.statItem}>
        <Statistic title="项目数" value={56} />
      </div>
      <div className={styles.statItem}>
        <Statistic title="团队内排名" value={8} suffix="/ 24" />
      </div>
      <div className={styles.statItem}>
        <Statistic title="项目访问" value={2223} />
      </div>
    </div>
  );
};
const Workplace: FC = () => {
  const { styles } = useStyles();
  const { loading: projectLoading, data: projectNotice = [] } =
    useRequest(queryProjectNotice);
  const { loading: activitiesLoading, data: activities = [] } =
    useRequest(queryActivities);
  const { data } = useRequest(fakeChartData);
  const renderActivities = (item: ActivitiesType) => {
    const events = item.template.split(/@\{([^{}]*)\}/gi).map((key) => {
      if (item[key as keyof ActivitiesType]) {
        const value = item[key as 'user'];
        return (
          <a href={value?.link} key={value?.name}>
            {value.name}
          </a>
        );
      }
      return key;
    });
    return (
      <List.Item key={item.id}>
        <List.Item.Meta
          avatar={<Avatar src={item.user.avatar} />}
          title={
            <span>
              <a className={styles.username}>{item.user.name}</a>
              &nbsp;
              <span className={styles.event}>{events}</span>
            </span>
          }
          description={
            <span className={styles.datetime} title={item.updatedAt}>
              {dayjs(item.updatedAt).fromNow()}
            </span>
          }
        />
      </List.Item>
    );
  };

  return (
    <PageContainer
      className="workplace-page"
      content={
        <PageHeaderContent
          currentUser={{
            avatar:
              'https://gw.alipayobjects.com/zos/rmsportal/BiazfanxmamNRoxxVxka.png',
            name: '吴彦祖',
            userid: '00000001',
            email: 'antdesign@alipay.com',
            signature: '海纳百川，有容乃大',
            title: '交互专家',
            group: '蚂蚁金服－某某某事业群－某某平台部－某某技术部－UED',
          }}
        />
      }
      extraContent={<ExtraContent />}
    >
      {/** 注入与分析/监控页一致的“深色渐变 + 半透明 + 玻璃效果”样式常量 */}
      {/** @ts-ignore */}
      {(() => {
        const anyWin: any = window as any;
        if (!anyWin.__panelStyles) {
          anyWin.__panelStyles = {
            panelStyle: {
background: 'linear-gradient(180deg, rgba(7,16,35,0.65) 0%, rgba(7,16,35,0.35) 100%)',
              border: '1px solid rgba(255,255,255,0.12)',
              borderRadius: 12,
              boxShadow: '0 10px 30px rgba(0,0,0,0.25)',
              backdropFilter: 'blur(6px)',
              WebkitBackdropFilter: 'blur(6px)',
              overflow: 'hidden',
            },
            headStyle: {
              background: 'transparent',
              color: '#E6F7FF',
              borderBottom: '1px solid rgba(255,255,255,0.08)',
              fontWeight: 600,
            },
            bodyStyle: {
              background: 'transparent',
              padding: 16,
              color: '#E6F7FF',
            },
          };
        }
        return null;
      })()}
      <Row gutter={24}>
        <Col xl={16} lg={24} md={24} sm={24} xs={24}>
          <Card
            className={styles.projectList}
            style={{ ...(window as any).__panelStyles?.panelStyle, marginBottom: 24 }}
            title="进行中的项目"
            headStyle={(window as any).__panelStyles?.headStyle}
            bodyStyle={(window as any).__panelStyles?.bodyStyle}
            bordered={false}
            extra={<Link to="/">全部项目</Link>}
            loading={projectLoading}
          >
            {projectNotice.map((item) => (
              <Card.Grid className={styles.projectGrid} key={item.id}>
                <Card.Meta
                  title={
                    <div className={styles.cardTitle}>
                      <Avatar size="small" src={item.logo} />
                      <Link to={item.href || '/'}>{item.title}</Link>
                    </div>
                  }
                  description={item.description}
                  style={{
                    width: '100%',
                  }}
                />
                <div className={styles.projectItemContent}>
                  <Link to={item.memberLink || '/'}>{item.member || ''}</Link>
                  {item.updatedAt && (
                    <span className={styles.datetime} title={item.updatedAt}>
                      {dayjs(item.updatedAt).fromNow()}
                    </span>
                  )}
                </div>
              </Card.Grid>
            ))}
          </Card>
          <Card
            styles={{
              body: {
                padding: activitiesLoading ? 16 : 0,
              },
            }}
            className={styles.activeCard}
            title="动态"
            style={{ ...(window as any).__panelStyles?.panelStyle, marginBottom: 24 }}
            headStyle={(window as any).__panelStyles?.headStyle}
            bodyStyle={(window as any).__panelStyles?.bodyStyle}
            bordered={false}
            loading={activitiesLoading}
          >
            <List<ActivitiesType>
              loading={activitiesLoading}
              renderItem={(item) => renderActivities(item)}
              dataSource={activities}
              className={styles.activitiesList}
              size="large"
            />
          </Card>
        </Col>
        <Col xl={8} lg={24} md={24} sm={24} xs={24}>
          <Card
            style={{ ...(window as any).__panelStyles?.panelStyle, marginBottom: 24 }}
            title="快速开始 / 便捷导航"
            headStyle={(window as any).__panelStyles?.headStyle}
            bodyStyle={(window as any).__panelStyles?.bodyStyle}
            bordered={false}
          >
            <EditableLinkGroup
              onAdd={() => {}}
              links={links}
              linkElement={Link}
            />
          </Card>
          <Card
            style={{ ...(window as any).__panelStyles?.panelStyle, marginBottom: 24 }}
            title="XX 指数"
            headStyle={(window as any).__panelStyles?.headStyle}
            bodyStyle={(window as any).__panelStyles?.bodyStyle}
            bordered={false}
            loading={data?.radarData?.length === 0}
          >
            <Radar
              height={343}
              data={data?.radarData || []}
              xField="label"
              colorField="name"
              yField="value"
              shapeField="smooth"
              area={{
                style: {
                  fillOpacity: 0.4,
                },
              }}
              axis={{
                x: {
                  labelFill: '#E6F7FF',
                  lineStroke: 'rgba(255,255,255,0.25)',
                  gridStroke: 'rgba(255,255,255,0.12)',
                },
                y: {
                  gridStrokeOpacity: 0.5,
                  labelFill: '#E6F7FF',
                  gridStroke: 'rgba(255,255,255,0.18)',
                },
              }}
              legend={{
                color: {
                  position: 'bottom',
                  layout: { justifyContent: 'center' },
                  itemLabelFill: '#E6F7FF',
                },
              }}
            />
          </Card>
          <Card
            styles={{
              body: {
                paddingTop: 12,
                paddingBottom: 12,
              },
            }}
            title="团队"
            style={{ ...(window as any).__panelStyles?.panelStyle, marginBottom: 24 }}
            headStyle={(window as any).__panelStyles?.headStyle}
            bodyStyle={(window as any).__panelStyles?.bodyStyle}
            bordered={false}
            loading={projectLoading}
          >
            <div className={styles.members}>
              <Row gutter={48}>
                {projectNotice.map((item) => {
                  return (
                    <Col span={12} key={`members-item-${item.id}`}>
                      <a>
                        <Avatar src={item.logo} size="small" />
                        <span className={styles.member}>
                          {item.member.substring(0, 3)}
                        </span>
                      </a>
                    </Col>
                  );
                })}
              </Row>
            </div>
          </Card>
        </Col>
      </Row>
    </PageContainer>
  );
};
export default Workplace;
