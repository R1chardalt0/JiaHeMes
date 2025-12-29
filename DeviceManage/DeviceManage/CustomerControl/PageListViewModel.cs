using System.Collections.ObjectModel;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceManage.CustomerControl;

public class PageListViewModel : BindableBase
{

    private int _total;
    public int Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    private int _pageSize;
    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    private int _pageIndex;
    public int PageIndex
    {
        get => _pageIndex;
        set => SetProperty(ref _pageIndex, value);
    }

    private int _totalPage;

    /// <summary>
    /// 
    /// </summary>
    public int TotalPage
    {
        get => _totalPage;
        set => SetProperty(ref _totalPage, value);
    }

    public PageListViewModel()
    {
        PageSizeChangedCommand = new(PageSizeChanged);
    }

    /// <summary>
    /// 分页按钮点击时，触发对外绑定的事件
    /// </summary>
    public DelegateCommand PageChangedCommand { get; set; }

    /// <summary>
    /// 分页大小发生改变时
    /// </summary>
    public DelegateCommand PageSizeChangedCommand { get; set; }

    void PageSizeChanged()
    {
        PageIndex = 1;
        PageChangedCommand.Execute();
    }

    public ObservableCollection<PageButtonItem> Buttons { get; set; } = new();

    /// <summary>
    /// 页面上最多显示几个页码按钮
    /// </summary>
    private int maxButtonCount = 9;
    /// <summary>
    /// 记录最大的页码值
    /// <remarks>判断是否点击的是最后一个页码，如果是，则需要更新所有按钮的页码</remarks>
    /// </summary>
    private int lastPageNum;
    /// <summary>
    /// 记录第一个页码值
    /// </summary>
    private int firstPageNum;
    /// <summary>
    /// 进行页码初始化
    /// </summary>
    public void Init()
    {
        Buttons.Clear();
        TotalPage = Convert.ToInt32(Math.Ceiling(Total / 1.0 / PageSize));
        var actualCount = TotalPage >= maxButtonCount ? maxButtonCount : TotalPage;
        lastPageNum = actualCount;
        GeneratePageButton(PageIndex, actualCount);
    }
    /// <summary>
    /// 生成页码
    /// </summary>
    /// <param name="loopStart"></param>
    /// <param name="loopEnd"></param>
    void GeneratePageButton(int loopStart, int loopEnd)
    {
        for (int i = loopStart; i <= loopEnd; i++)
        {
            Buttons.Add(new()
            {
                Content = i.ToString(),
                ButtonStyle = Application.Current.TryFindResource(PageIndex == i ? "BlueBtn" : "SkyBtn") as Style,
                PageClickCommand = new(PageClick)
            });
        }
        firstPageNum = loopStart;
        lastPageNum = loopEnd;
    }

    // 点击页码
    void PageClick(object sender)
    {
        // 如果点击的是最后一个按钮，则页码需要重新生成
        var cornerBtn = sender as CornerButton;
        PageIndex = Convert.ToInt32(cornerBtn.Content);
        // 点击页面上第一个页码，需要重新生成新的页码
        if (firstPageNum == PageIndex)
        {
            ClickFirstPageButton(cornerBtn);
        }
        // 点击页面上最后一个页码，需要重新生成新的页码
        else if (PageIndex == lastPageNum && TotalPage != PageIndex)
        {
            ClickLastPageButton();
        }
        else
        {
            // 还原按钮样式
            RestoreStyle(cornerBtn);
        }

        // 触发分页事件
        PageChangedCommand.Execute();
    }
    /// <summary>
    /// 点击页面上第一个页码，需要重新生成新的页码
    /// </summary>
    /// <param name="cornerBtn"></param>
    void ClickFirstPageButton(CornerButton cornerBtn)
    {
        if (PageIndex == 1)
        {
            RestoreStyle(cornerBtn);
            return;
        }

        Buttons.Clear();
        var startNum = PageIndex - maxButtonCount + 2;
        var loopEnd = PageIndex + 1;
        if (startNum <= 0)
        {
            startNum = 1;
            loopEnd = maxButtonCount;
        }

        GeneratePageButton(startNum, loopEnd);
    }

    /// <summary>
    /// 点击最后一个按钮
    /// </summary>
    void ClickLastPageButton()
    {
        Buttons.Clear();

        int loopStart = PageIndex - 1;
        var loopEnd = PageIndex + maxButtonCount - 2;

        // 最后一页
        if (PageIndex + maxButtonCount >= TotalPage)
        {
            loopStart = TotalPage - maxButtonCount + 1;
            loopEnd = TotalPage;
        }

        GeneratePageButton(loopStart, loopEnd);
    }

    /// <summary>
    /// 还原按钮样式
    /// </summary>
    /// <param name="cornerBtn"></param>
    private void RestoreStyle(CornerButton cornerBtn)
    {
        foreach (PageButtonItem btn in Buttons)
        {
            btn.ButtonStyle = Application.Current.TryFindResource("SkyBtn") as Style;
        }
        // 给当前点击的页码按钮不同的样式，以示区别
        cornerBtn.Style = Application.Current.TryFindResource("BlueBtn") as Style;
    }
}

public class PageButtonItem : BindableBase
{
    private string _content;

    /// <summary>
    /// 分页按钮内容
    /// </summary>
    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    private DelegateCommand<object> _pageClickCommand;

    /// <summary>
    /// 分页按钮点击事件
    /// </summary>
    public DelegateCommand<object> PageClickCommand
    {
        get => _pageClickCommand;
        set => SetProperty(ref _pageClickCommand, value);
    }

    private Style _buttonStyle = Application.Current.TryFindResource("SkyBtn") as Style;

    /// <summary>
    /// 按钮样式
    /// </summary>
    public Style ButtonStyle
    {
        get => _buttonStyle;
        set => SetProperty(ref _buttonStyle, value);
    }
}