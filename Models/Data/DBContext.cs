using Microsoft.EntityFrameworkCore;
using project_DBA_VISO.Models.Views;
using project_DBA_VISO.Models;
using Models.Views;
namespace project_DBA_VISO.Models.Data
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Currency> Currencies {  get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Supplier_Phone> Phones { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Invoice_Detail> Invoice_details { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User_Rol> User_Rols { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Purchase_Detail> Purchase_Details { get; set; }
        
        public DbSet<InvoiceViewModel> InvoiceViews {  get; set; }
        public DbSet<PurchaseViewModel> purchaseViews { get; set; }
        public DbSet<TotalCountsViewModel> TotalCounts { get; set; }
        public DbSet<TotalProfitLossViewModel> totalProfitLosses { get; set; }
        public DbSet<TotalMontoViewModel> TotalMontos {  get; set; }
        public DbSet<TotalMontoInvoice> montoInvoices { get; set; }
        public DbSet<Invoice_detail_clientViewModel> detail_ClientViewModels { get; set; }
        public DbSet<SalesReportViewModel> SalesReportViewModels { get; set; }
        public DbSet<PurchaseReportViewModel> PurchaseReportViewModels { get; set; }    
        public DbSet<TopProductsViewModel> TopProductsViews { get; set; }
        public DbSet<DashboardDataViewModel> DashboardDataViewModels { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().ToTable("Categoria");
            modelBuilder.Entity<Brand>().ToTable("Marca");
            modelBuilder.Entity<Currency>().ToTable("Tipo_Moneda");
            modelBuilder.Entity<Client>().ToTable("Cliente");
            modelBuilder.Entity<Supplier_Phone>().ToTable("Proveedor_Telefono");
            modelBuilder.Entity<Invoice>().ToTable("Factura");
            modelBuilder.Entity<Invoice_Detail>().ToTable("Detalle_Factura");
            modelBuilder.Entity<Product>().ToTable("Producto");
            modelBuilder.Entity<Supplier>().ToTable("Proveedor");
            modelBuilder.Entity<User_Rol>().ToTable("Rol_Usuario");
            modelBuilder.Entity<User>().ToTable("Usuario");
            modelBuilder.Entity<Purchase>().ToTable("Compra");
            modelBuilder.Entity<Purchase_Detail>().ToTable("Detalle_Compra");

            modelBuilder.Entity<Category>()
                .HasKey(c => c.Categoria_Id);
            modelBuilder.Entity<Brand>()
                .HasKey(b => b.Marca_Id);
            modelBuilder.Entity<Currency>()
                .HasKey(cu => cu.Moneda_Id);
            modelBuilder.Entity<Client>()
                .HasKey(cl => cl.Cliente_Id);
            modelBuilder.Entity<Supplier_Phone>()
                .HasKey(sp => sp.Id);
            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.Factura_Id);
            modelBuilder.Entity<Invoice_Detail>()
                .HasKey(iv => iv.DetalleFactura_Id);
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Producto_Id);
            modelBuilder.Entity<Supplier>()
                .HasKey(s => s.Proveedor_Id);
            modelBuilder.Entity<User_Rol>()
                .HasKey(ur => ur.Rol_id);
            modelBuilder.Entity<User>()
                .HasKey(u => u.Usuario_Id);
            modelBuilder.Entity<Purchase>()
                .HasKey(pu => pu.Compra_Id);
            modelBuilder.Entity<Purchase_Detail>()
                .HasKey(pd => pd.DetalleCompra_Id);

            modelBuilder.Entity<InvoiceViewModel>().HasNoKey();
            modelBuilder.Entity<PurchaseViewModel>().HasNoKey();
            modelBuilder.Entity<TotalCountsViewModel>().HasNoKey();
            modelBuilder.Entity<TotalProfitLossViewModel>().HasNoKey();
            modelBuilder.Entity<TotalMontoViewModel>().HasNoKey();
            modelBuilder.Entity<TotalMontoInvoice>().HasNoKey();
            modelBuilder.Entity<Invoice_detail_clientViewModel>().HasNoKey();
            modelBuilder.Entity<SalesReportViewModel>().HasNoKey();
            modelBuilder.Entity<PurchaseReportViewModel>().HasNoKey();
            modelBuilder.Entity<TopProductsViewModel>().HasNoKey();
            modelBuilder.Entity<DashboardDataViewModel>().HasNoKey();

        }
    }
}
