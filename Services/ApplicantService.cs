using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using AppTran.Models;
using Dapper;
using Microsoft.Extensions.Logging;

namespace AppTran.Services
{
    public class ApplicantService : IApplicantService
    {
        private readonly string _connectionString;
        private readonly ILogger<ApplicantService> _logger;

        public ApplicantService(IConfiguration configuration, ILogger<ApplicantService> logger)
        {
            _connectionString = configuration.GetConnectionString("MsSqlConnection");
            _logger = logger;
        }

        public async Task<bool> CheckRegNumberExistsAsync(string regNumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(1) FROM Package WHERE RegNumber = @RegNumber";
                var exists = await connection.QueryFirstAsync<int>(query, new { RegNumber = regNumber });
                return exists > 0;
            }
        }


        public async Task<List<ApplicantInfo>> SearchApplicantsAsync(string searchText)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                SELECT 
                    a.Name, 
                    a.Mail, 
                    a.OGRN,
                    dbo.fn_GetOrganizationByConfidentRegNumber(a.ConfidentRegNumber) as Company,
                    CASE
                        WHEN ac.NotAfter < GETDATE() THEN '����������� ' + CONVERT(varchar, ac.NotAfter, 105)
                        WHEN ac.NotAfter > GETDATE() THEN '��������� � ' + CONVERT(varchar, ac.NotBefore, 105) + ' �� ' + CONVERT(varchar, ac.NotAfter, 105)
                        ELSE '' 
                    END as CertificateStatus
                FROM Applicant a
                LEFT JOIN Applicant_Certificate ac ON ac.ApplicantId = a.Id
                WHERE a.Name LIKE @SearchText + '%'
                ORDER BY a.Name";

                    _logger.LogInformation($"Executing query with searchText: {searchText}");
                    var results = await connection.QueryAsync<ApplicantInfo>(query, new { SearchText = searchText });
                    var resultsList = results.AsList();
                    _logger.LogInformation($"Found {resultsList.Count} results");
                    return resultsList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SearchApplicantsAsync: {ex.Message}");
                throw;
            }
        }
    }
}