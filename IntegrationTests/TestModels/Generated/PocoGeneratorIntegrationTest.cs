﻿using System.Collections.Generic;
using IntegrationTests.TestModels.Generated.POCO;
using TestDataFramework.Populator.Concrete;
using TestDataFramework.Populator.Interfaces;

namespace IntegrationTests.TestModels.Generated
{
    public class PocoGeneratorIntegrationTest : ICodeGeneratorIntegration
    {
        private IList<RecordReference<TertiaryManualKeyForeignTable>> tertiaryForeignSet;
        private IList<RecordReference<ForeignToAutoPrimaryTable>> foreignToAutoSet;

        public void AddTypes(IPopulator populator, IList<RecordReference<ManualKeyForeignTable>> foreignSet1, IList<RecordReference<ManualKeyForeignTable>> foreignSet2)
        {
            Decorator.Decorate(populator);

            this.tertiaryForeignSet = populator.Add<TertiaryManualKeyForeignTable>(4);

            this.tertiaryForeignSet[0].AddPrimaryRecordReference(foreignSet1[0]);
            this.tertiaryForeignSet[1].AddPrimaryRecordReference(foreignSet1[1]);
            this.tertiaryForeignSet[2].AddPrimaryRecordReference(foreignSet2[0]);
            this.tertiaryForeignSet[3].AddPrimaryRecordReference(foreignSet2[1]);

            this.foreignToAutoSet = populator.Add<ForeignToAutoPrimaryTable>(2);

            this.foreignToAutoSet[0].AddPrimaryRecordReference(this.tertiaryForeignSet[0]);
            this.foreignToAutoSet[1].AddPrimaryRecordReference(this.tertiaryForeignSet[1]);
        }

        public void Dump()
        {
            Helpers.Dump(this.tertiaryForeignSet);
            Helpers.Dump(this.foreignToAutoSet);
        }
    }
}
