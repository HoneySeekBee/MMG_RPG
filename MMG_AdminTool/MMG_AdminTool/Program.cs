namespace MMG_AdminTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews(); // ��Ʈ�ѷ��� �信 ���� ���� 
            
            var app = builder.Build();
            
            app.MapControllerRoute(
                name:"default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
                );


            app.Run();
        }
    }
}
