﻿using EFCore.PostgresExtensions.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.PostgresExtensions.Extensions;

public static class MigrationBuilderExtensions
{
   public static void CreateRandomIdSequence(this MigrationBuilder migrationBuilder,
      string tableName,
      string pkName,
      long startValue,
      int minRandIncrementValue,
      int maxRandIncrementValue)
   {
      migrationBuilder.Sql(PgFunctionHelpers.GetPgFunction(tableName,
         pkName,
         startValue,
         minRandIncrementValue,
         maxRandIncrementValue));
   }
}