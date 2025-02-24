
using Microsoft.EntityFrameworkCore;
using NoteKeeper.Aplicacao.ModuloCategoria;
using NoteKeeper.Aplicacao.ModuloNota;
using NoteKeeper.Dominio.Compartilhado;
using NoteKeeper.Dominio.ModuloCategoria;
using NoteKeeper.Dominio.ModuloNota;
using NoteKeeper.Infra.Orm.Compartilhado;
using NoteKeeper.Infra.Orm.ModuloCategoria;
using NoteKeeper.Infra.Orm.ModuloNota;
using NoteKeeper.WebApi.Config.Mapping;
using NoteKeeper.WebApi.Config.Mapping.Actions;

namespace NoteKeeper.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            const string politicaCors = "_minhaPoliticaCors";

            //Configura��o de Inje��o de Depend�ncia
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("sqlServer");

            builder.Services.AddDbContext<IContextoPersistencia, NoteKeeperDbContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServer(connectionString, dbOptions =>
                {
                    dbOptions.EnableRetryOnFailure();
                });
            });

            builder.Services.AddScoped<IRepositorioCategoria, RepositorioCategoriaOrm>();
            builder.Services.AddScoped<ServicoCategoria>();

            builder.Services.AddScoped<IRepositorioNota, RepositorioNotaOrm>();
            builder.Services.AddScoped<ServicoNota>();

            builder.Services.AddScoped<ConfigurarCategoriaMappingAction>();

            builder.Services.AddAutoMapper(config =>
            {
                config.AddProfile<CategoriaProfile>();
                config.AddProfile<NotaProfile>();
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: politicaCors, policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddControllers();  
            
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();


            //Middleware de execu��o da API
            var app = builder.Build();            
            
            app.UseSwagger();
            app.UseSwaggerUI();

            //Migra��es de Banco de Dados
            {
                 using var scope = app.Services.CreateScope();

                 var dbCotnext = scope.ServiceProvider.GetRequiredService<IContextoPersistencia>();

                if(dbCotnext is NoteKeeperDbContext dbContext)
                {
                    MigradorBancoDados.AtualizarBancoDados(dbContext);
                }
            }

            app.UseHttpsRedirection();

            app.UseCors(politicaCors);

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
