// 配置文件
const Config = {
    // API基础URL
    API_BASE_URL: 'http://localhost:8001'
};

// 暴露配置对象
try {
    // 浏览器环境
    window.Config = Config;
} catch (e) {
    // Node.js环境
    module.exports = Config;
}