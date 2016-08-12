using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SimchaFund.Data1
{
    public class SimchosManager
    {
        private string _connString;
        public SimchosManager(string connString)
        {
            _connString = connString;
        }

        //---------------------------Simchos Page --------------------------------------//

        public IEnumerable<SimchaWithContrCount> GetSimchosWithCount()
        {
            var simchos = new List<SimchaWithContrCount>();
            DBAction(cmd =>
                {
                    cmd.CommandText = "select COUNT(c.id) as contrcount, s.BaalSimcha, s.Date, s.total, s.id " +
                                       " from simchos s left join Contributions m  " +
                                        "on s.id = m.SimchaId left join Contributors c " +
                                        "on c.Id = m.ContributorId group by s.BaalSimcha, s.Date, s.total, s.id";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        simchos.Add(new SimchaWithContrCount
                        {
                            Count = (int)reader["contrcount"],
                            BaalSimcha = (string)reader["baalsimcha"],
                            Date = (DateTime)reader["date"],
                            Total = (int?)reader["total"],
                            Id = (int)reader["id"]
                        });
                    }
                });
            return simchos;
        }

        public int GetTotalContributors()
        {
            int total = 0;
            DBAction(cmd =>
                {
                    cmd.CommandText = "SELECT count(*) FROM Contributors";
                    total = (int)cmd.ExecuteScalar();
                });
            return total;
        }

        public void AddSimcha(Simcha simcha)
        {
            if (simcha.BaalSimcha == null
                 || simcha.Date == null)
            {
                return;
            }
            DBAction(cmd =>
                {
                    cmd.CommandText = "INSERT INTO Simchos (BaalSimcha, Date, Total) " +
                                      "VALUES (@baalsimcha, @date, 0)";
                    cmd.Parameters.AddWithValue("@baalsimcha", simcha.BaalSimcha);
                    cmd.Parameters.AddWithValue("@date", simcha.Date);
                    cmd.ExecuteNonQuery();
                });
        }
        //----------------------- List of Contributions for each Simcha ------------------

        public Simcha GetSimchaById(int simchaid)
        {
            var simcha = new Simcha();
            DBAction(cmd =>
                {
                    cmd.CommandText = "SELECT * FROM Simchos WHERE  simchos.id = @simchaid";
                    cmd.Parameters.AddWithValue("@simchaid", simchaid);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        simcha.BaalSimcha = (string)reader["baalsimcha"];
                        simcha.Id = (int)reader["id"];
                        simcha.Date = (DateTime)reader["date"];
                        simcha.Total = (int?)reader["total"];
                    }
                });
            return simcha;
        }

        public IEnumerable<Contributor> GetSimchaContributions(int simchaId)
        {
            var cont = new List<Contributor>();
            DBAction(cmd =>
                {
                    cmd.CommandText = "SELECT c.Id, c.FirstName, c.lastname, c.AlwaysInclude " +
                                        "FROM Contributors c JOIN Contributions m " +
                                       " ON c.id = m.ContributorId AND m.simchaid = @simchaid " +
                                        "GROUP BY c.FirstName, c.lastname, c.id, c.alwaysinclude";
                    cmd.Parameters.AddWithValue("@simchaid", simchaId);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cont.Add(new Contributor
                        {
                            Id = (int)reader["id"],
                            FirstName = (string)reader["firstname"],
                            LastName = (string)reader["lastname"],
                            AlwaysInclude = (bool)reader["alwaysinclude"]
                        });
                    };
                });
            return cont;
        }


        public void UpdateSimchaContributions(int simchaId, List<SimchaContribution> includedContributors)
        {
            List<SimchaContribution> prevContributions = getContributionAmounts(simchaId);
            foreach (SimchaContribution contribution in includedContributors)
            {
                SimchaContribution prevContribution = prevContributions.FirstOrDefault(c => c.Id == contribution.Id);
                if (prevContribution != null)
                {
                    if (contribution.Amount != prevContribution.Amount)
                    {
                        UpdateContributionAmount(contribution.Id, simchaId, contribution.Amount);
                    }
                    if (!contribution.Included)
                    {
                        RemoveContribution(contribution.Id, simchaId);
                    }
                }
                else if (prevContribution == null && contribution.Included)
                   {
                       AddSimchaContribution(contribution.Id, simchaId, contribution.Amount);
                   }
            }
        }

        public List<SimchaContribution> getContributionAmounts(int simchaId)
        {
            List<SimchaContribution> contributions = new List<SimchaContribution>();
            DBAction(cmd =>
               {
                   cmd.CommandText = "SELECT * FROM Contributions " +
                                     "WHERE SimchaId = @simchaId";
                   cmd.Parameters.AddWithValue("@simchaId", simchaId);
                   var reader = cmd.ExecuteReader();
                   while (reader.Read())
                   {
                          int Id = (int)reader["contributorId"];
                          int Amount = (int)reader["contributionamount"];
                          DateTime Date = (DateTime)reader["date"];
                      if (Id != 0)
                      {
                          contributions.Add(new SimchaContribution
                              {
                                  Id = Id,
                                  Amount = Amount,
                                  Date = Date
                              });
                      }
                   }

               });
            return contributions;
        }

        public void UpdateContributionAmount(int contributorId, int simchaId, int amount)
        {
            DBAction(cmd =>
                {
                    cmd.CommandText = "UPDATE contributions " +
                    "SET ContributionAmount = @amount " +
                    "WHERE contributorid = @contrId AND simchaid = @simchaId";
                    cmd.Parameters.AddWithValue("@contrId", contributorId);
                    cmd.Parameters.AddWithValue("@simchaId", simchaId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.ExecuteNonQuery();
                });
        }
        public void AddSimchaContribution(int contributorId, int simchaId, int amount)
        {
            DBAction(cmd =>
                {
                    cmd.CommandText = "INSERT INTO Contributions (ContributorId, SimchaId, Date, ContributionAmount) " +
                                      "VALUES (@contrId, @simchaId, @date, @contrAmount) ";
                    cmd.Parameters.AddWithValue("@contrId", contributorId);
                    cmd.Parameters.AddWithValue("@simchaId", simchaId);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@contrAmount", amount);
                    cmd.ExecuteNonQuery();
                });
        }
        public void RemoveContribution(int contrId, int simchaId)
        {
            DBAction(cmd =>
                {
                    cmd.CommandText = "delete from  contributions   " +
                    "where contributorid  = @contrId " +
                    "and simchaid = @simchaId";
                    cmd.Parameters.AddWithValue("@contrId", contrId);
                    cmd.Parameters.AddWithValue("@simchaId", simchaId);
                    cmd.ExecuteNonQuery();
                });
        }
        //------------------------- Contributors   ---------------------------

        public IEnumerable<Contributor> GetContributors()
        {
            var contr = new List<Contributor>();
            DBAction(cmd =>
            {
                cmd.CommandText = "SELECT  * FROM Contributors";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    contr.Add(new Contributor
                    {
                        FirstName = (string)reader["firstname"],
                        LastName = (string)reader["lastname"],
                        CellNumber = (string)reader["cellnumber"],
                        Id = (int)reader["id"],
                        AlwaysInclude = (bool)reader["alwaysinclude"],
                    });
                }
            });
            foreach (Contributor c in contr)
            {
                c.Balance = GetContributorBalance(c.Id);
            }
            return contr;
        }
        public IEnumerable<Contributor> GetContributors(string query)
        {
            var contr = new List<Contributor>();
            DBAction(cmd =>
            {
                cmd.CommandText = "SELECT * FROM contributors    " +
                                 " WHERE firstname LIKE @query  " +
                                 " OR lastname LIKE @query";
                cmd.Parameters.AddWithValue("@query", "%" + query + "%");
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    contr.Add(new Contributor
                    {
                        FirstName = (string)reader["firstname"],
                        LastName = (string)reader["lastname"],
                        CellNumber = (string)reader["cellnumber"],
                        Id = (int)reader["id"],
                        AlwaysInclude = (bool)reader["alwaysinclude"],
                    });
                }
            });
            foreach (Contributor c in contr)
            {
                c.Balance = GetContributorBalance(c.Id);
            }
            return contr;
        }

        public void AddContributor(Contributor c)
        {
            DBAction(cmd =>
                {
                    cmd.CommandText = "INSERT INTO Contributors (Firstname, lastname, cellnumber, alwaysinclude) " +
                                      "VALUES (@firstname, @lastname, @cellnumber, @alwaysinclude)";
                    cmd.Parameters.AddWithValue("@firstname", c.FirstName);
                    cmd.Parameters.AddWithValue("@lastname", c.LastName);
                    cmd.Parameters.AddWithValue("@cellnumber", c.CellNumber);
                    cmd.Parameters.AddWithValue("@alwaysinclude", c.AlwaysInclude);
                    cmd.ExecuteNonQuery();
                });
        }

        public void EditContributor(Contributor c)
        {
            DBAction(cmd =>
                {
                    cmd.CommandText = "update contributors " +
                            "set firstname = @firstname, lastname = @lastname, cellnumber = @cellnumber, alwaysinclude = @alwaysinclude " +
                           " where id = @id";
                    cmd.Parameters.AddWithValue("@firstname", c.FirstName);
                    cmd.Parameters.AddWithValue("@lastname", c.LastName);
                    cmd.Parameters.AddWithValue("@cellnumber", c.CellNumber);
                    cmd.Parameters.AddWithValue("@alwaysinclude", c.AlwaysInclude);
                    cmd.Parameters.AddWithValue("@id", c.Id);
                    cmd.ExecuteNonQuery();

                });
        }

        public int GetContributorBalance(int id)
        {
            var deposittotal = 0;
            var contributiontotal = 0;
            DBAction(cmd =>
                {
                    cmd.CommandText = "SELECT ISNULL(Sum(Amount), 0) FROM Deposit WHERE ContributorId = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    deposittotal = (int)cmd.ExecuteScalar();
                });
            DBAction(cmd =>
            {
                cmd.CommandText = "SELECT ISNULL(Sum(contributionAmount), 0) FROM Contributions WHERE contributorId = @id";
                cmd.Parameters.AddWithValue("@id", id);
                contributiontotal = (int)cmd.ExecuteScalar();
            });
            return deposittotal - contributiontotal;
        }

        public IEnumerable<Transaction> GetContributorHistory(int contributorId)
        {
            List<Transaction> transactions = new List<Transaction>();
            DBAction(cmd =>
                {
                    cmd.CommandText = "SELECT * FROM DEPOSIT WHERE ContributorId = @id";
                    cmd.Parameters.AddWithValue("@id", contributorId);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Type = TransactionType.Deposit,
                            Date = (DateTime)reader["Date"],
                            Amount = (int)reader["Amount"]
                        });
                    }
                });
            DBAction(cmd =>
                {
                    cmd.CommandText = "SELECT m.*, s.BaalSimcha   FROM Contributions m " +
                                      "JOIN simchos s on s.Id = m.SimchaId " +
                                      "WHERE ContributorId = @id";
                    cmd.Parameters.AddWithValue("@id", contributorId);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Type = TransactionType.Contribution,
                            SimchaName = (string)reader["BaalSimcha"],
                            Date = (DateTime)reader["Date"],
                            Amount = (int)reader["ContributionAmount"]
                        });
                    }
                });
            return transactions.OrderBy(t => t.Date);
        }

        public int Deposit(int id, int amount, DateTime date)
        {
            DBAction(cmd =>
                {
                    cmd.CommandText = "INSERT INTO deposit (date, contributorid, amount)" +
                                       "VALUES (@date, @id, @amount)";
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.ExecuteNonQuery();
                });

            return GetContributorBalance(id);
        }
        //------------------------- Shared  ---------------------------
        public void DBAction(Action<SqlCommand> action)
        {
            using (SqlConnection connection = new SqlConnection(_connString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                connection.Open();
                action(cmd);
            }
        }
    }
}
