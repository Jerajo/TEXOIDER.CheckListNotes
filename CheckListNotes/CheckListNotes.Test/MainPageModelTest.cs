using Xunit;
using CheckListNotes.PageModels;

namespace CheckListNotes.Test
{
    public class MainPageModelTest
    {
        private CheckListPageModel Page { get; set; }

        public MainPageModelTest()
        {
            Page = new CheckListPageModel();
            Page.Init(null);
        }

        [Theory]
        [InlineData(1,true)]
        [InlineData(2,false)]
        [InlineData(3,true)]
        public void TaskChecked(int index, bool isCheced)
        {
            var list = Page.Tasks;
            var checkedList = Page.CheckedTasks;

            var model1 = list[index];
            var model2 = checkedList[index];

            model1.IsChecked = isCheced;
            model1.IsChecked = !isCheced;
            model2.IsChecked = isCheced;
            model2.IsChecked = !isCheced;

            Assert.True(model1?.IsValid);
            Assert.True(model2?.IsValid);
            Assert.True(list.IndexOf(model1) == -1);
            Assert.True(list.IndexOf(model2) != -1);
            Assert.True(checkedList.IndexOf(model2) == -1);
            Assert.True(checkedList.IndexOf(model1) != -1);
        }

    }
}
