using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using TrailerModels;
using System.Data.Entity.Core.Objects;

namespace EF_Trailer
{
    public class Trailer: DbContext
    {
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public bool SaveAudits = false;
        public new DbChangeTracker ChangeTracker;
        dynamic User;

        public override int  SaveChanges()
        {
            if (SaveAudits == false)
            {
                return base.SaveChanges();
            }


            var addedAuditedEntities = ChangeTracker.Entries()
                                        .Where(p => p.State == EntityState.Added)
                                        .Select(p => p.Entity).ToList();

            var modifiedAuditedEntities = ChangeTracker.Entries()
                                          .Where(p => p.State == EntityState.Modified)
                                          .Select(p => p.Entity).ToList();


            var deletedAuditedEntities = ChangeTracker.Entries()
                                        .Where(p => p.State == EntityState.Deleted)
                                        .Select(p => p.Entity).ToList();

            var unmo = ChangeTracker.Entries()
                                        .Where(p => p.State == EntityState.Added).ToList();


            var now = DateTime.UtcNow;

            // Serializer settings
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CustomResolver();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Formatting = Formatting.Indented;



            foreach (var modified in modifiedAuditedEntities)
            {

                var keys = GetKey(modified, this).Split(',');
                object[] keyvalues = new object[keys.Count()];

                for (int i = 0; i < keys.Count(); i++)
                {
                    keyvalues[i] = modified.GetType().GetProperty(keys[i]).GetValue(modified);
                }

                Trailer asdf = new Trailer();

                var entity2 = asdf.Set(modified.GetType()).Find(keyvalues);



                AuditTrail trail = new AuditTrail()
                {
                    Action = EntityObjectState.Modified,
                    AuditDateTime = now,
                    EntityType = ObjectContext.GetObjectType(modified.GetType()).Name,
                    OriginalEntity = JsonConvert.SerializeObject(entity2, settings),
                    NewEntity = JsonConvert.SerializeObject(modified, settings),
                    UserId = (string)User.UserId,

                };

                this.AuditTrails.Add(trail);
            }


            var returnvalue = base.SaveChanges();


            foreach (var added in addedAuditedEntities)
            {
                AuditTrail trail = new AuditTrail()
                {
                    Action = EntityObjectState.Added,
                    AuditDateTime = now,
                    EntityType = added.GetType().Name,
                    OriginalEntity = JsonConvert.SerializeObject(added, settings),
                    UserId = (string)User.UserId,

                };

                this.AuditTrails.Add(trail);

            }


            foreach (var deleted in deletedAuditedEntities)
            {
                AuditTrail trail = new AuditTrail()
                {
                    Action = EntityObjectState.Deleted,
                    AuditDateTime = now,
                    EntityType = ObjectContext.GetObjectType(deleted.GetType()).Name,


                    OriginalEntity = JsonConvert.SerializeObject(deleted, settings),
                    UserId = (string)User.UserId,

                };

                this.AuditTrails.Add(trail);

            }

            base.SaveChanges();
            return returnvalue;
        }

        public static string GetKey(dynamic entity, Trailer Context)
        {
            // EF CORE:
            //var keyName = String.Join(",", Context.Model.FindEntityType(entity.GetType()).FindPrimaryKey().Properties
            //    .Select(x => x.Name));

            ObjectContext objectContext = ((IObjectContextAdapter)Context).ObjectContext;

            MethodInfo method = typeof(ObjectContext).GetMethod("CreateObjectSet", new Type[] { });
            MethodInfo generic = method.MakeGenericMethod(ObjectContext.GetObjectType(entity.GetType()));
            dynamic set = generic.Invoke(objectContext, null);

            string keyNames = String.Join(",", ((IEnumerable<dynamic>)set.EntitySet.ElementType
                                                        .KeyMembers).Select(k => k.Name).ToList());

            return keyNames;
        }

    }

    public class CustomResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);

            if ((prop.DeclaringType != typeof(Trailer) &&
                prop.PropertyType.IsClass &&
                prop.PropertyType != typeof(string)) || prop.PropertyType.IsGenericType || prop.Writable == false)
            {
                prop.ShouldSerialize = obj => false;
            }

            return prop;

        }

    }
}

