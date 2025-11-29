using Microsoft.AspNetCore.Mvc;

namespace SIMS.Controllers
{
    public class TestController : Controller
    {
        public string Index(int id, string className, string school)
        {
            // id, className, school : tham so truyen tu url trinh duyet
            return $"Hello SE07201 - BTec : id = {id}, class = {className}, School = {school}";
        }
        // test/index?id=10&className=se07201&school=btec : chay len url cua trinh duyet
        public string DetailProduct(int productId = 0)
        {
            return $"ID of product is : {productId}";
            // test/DetailProduct?productId=999
        }
        public IActionResult Login()
        {
            return View(); // hien thi ra 1 giao dien(html,css,js ...)
        }
    }
}
