
import { Presentation, Reference } from '@/types/presentation';

// NOTE: This mock data is only used for demonstration purposes.
// In a real application, this would be replaced with actual data from the uploaded presentation.
export const mockPresentation: Presentation = {
  id: '1',
  title: 'Type 2 Diabetes Management',
  author: 'Dr. Jane Smith',
  originalFileName: 'diabetes_management_2022.pptx',
  uploadDate: new Date('2023-04-06'),
  isAnalysisComplete: true,
  slides: [
    {
      id: 's1',
      number: 1,
      title: 'Introduction',
      originalContent: 'Type 2 Diabetes affects approximately 10% of the adult population worldwide. This presentation covers current management strategies based on the 2022 guidelines.',
      suggestedUpdate: 'Type 2 Diabetes affects approximately 11.3% of the adult population worldwide, with rates continuing to rise. This presentation covers current management strategies based on the latest 2023 guidelines.',
      updateReason: 'Updated prevalence statistics based on the latest WHO Global Diabetes Report.',
      sourceCitations: [
        'World Health Organization. (2023). Global Diabetes Report 2023.',
        'American Diabetes Association. (2023). Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1).'
      ],
      status: 'pending'
    },
    {
      id: 's2',
      number: 2,
      title: 'Diagnostic Criteria',
      originalContent: 'Diagnostic criteria for T2DM include:\n- Fasting plasma glucose ≥ 126 mg/dL\n- 2-hour plasma glucose ≥ 200 mg/dL during OGTT\n- HbA1c ≥ 6.5%\n- Random plasma glucose ≥ 200 mg/dL in patients with symptoms',
      suggestedUpdate: 'Diagnostic criteria for T2DM include:\n- Fasting plasma glucose ≥ 126 mg/dL (7.0 mmol/L)\n- 2-hour plasma glucose ≥ 200 mg/dL (11.1 mmol/L) during OGTT\n- HbA1c ≥ 6.5% (48 mmol/mol)\n- Random plasma glucose ≥ 200 mg/dL (11.1 mmol/L) in patients with symptoms',
      updateReason: 'Added SI units (mmol/L and mmol/mol) alongside traditional units for international compatibility.',
      sourceCitations: [
        'American Diabetes Association. (2023). 2. Classification and diagnosis of diabetes: Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1), S19-S40.'
      ],
      status: 'pending'
    },
    {
      id: 's3',
      number: 3,
      title: 'First-Line Treatments',
      originalContent: 'Metformin remains the first-line agent for most patients with Type 2 Diabetes.',
      suggestedUpdate: 'Metformin remains the first-line agent for most patients with Type 2 Diabetes. However, the 2023 ADA Standards of Care now recommend considering GLP-1 receptor agonists or SGLT2 inhibitors as first-line therapy in patients with established ASCVD, heart failure, or CKD, even without metformin.',
      updateReason: 'The American Diabetes Association updated their guidelines in 2023 to emphasize cardiorenal protection.',
      sourceCitations: [
        'American Diabetes Association. (2023). 9. Pharmacologic approaches to glycemic treatment: Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1), S140-S157.',
        'American Diabetes Association. (2023). 10. Cardiovascular disease and risk management: Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1), S158-S190.'
      ],
      status: 'pending'
    }
  ]
};

export const mockReferences: Reference[] = [
  {
    id: 'r1',
    citation: 'World Health Organization. (2023). Global Diabetes Report 2023.',
    year: 2023,
    journal: 'World Health Organization',
    type: 'guideline'
  },
  {
    id: 'r2',
    citation: 'American Diabetes Association. (2023). Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1).',
    year: 2023,
    journal: 'Diabetes Care',
    type: 'guideline'
  },
  {
    id: 'r3',
    citation: 'American Diabetes Association. (2023). 2. Classification and diagnosis of diabetes: Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1), S19-S40.',
    year: 2023,
    journal: 'Diabetes Care',
    type: 'guideline'
  },
  {
    id: 'r4',
    citation: 'American Diabetes Association. (2023). 9. Pharmacologic approaches to glycemic treatment: Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1), S140-S157.',
    year: 2023,
    journal: 'Diabetes Care',
    type: 'guideline'
  },
  {
    id: 'r5',
    citation: 'American Diabetes Association. (2023). 10. Cardiovascular disease and risk management: Standards of Medical Care in Diabetes. Diabetes Care, 46(Supplement 1), S158-S190.',
    year: 2023,
    journal: 'Diabetes Care',
    type: 'guideline'
  },
  {
    id: 'r6',
    citation: 'Davies MJ, Aroda VR, Collins BS, et al. Management of Hyperglycemia in Type 2 Diabetes, 2022. A Consensus Report by the American Diabetes Association (ADA) and the European Association for the Study of Diabetes (EASD). Diabetes Care. 2022;45(11):2753-2786.',
    year: 2022,
    journal: 'Diabetes Care',
    type: 'guideline'
  },
  {
    id: 'r7',
    citation: 'Lingvay I, Sumithran P, Cohen RV, le Roux CW. Obesity management as a primary treatment goal for type 2 diabetes: time to reframe the conversation. Lancet. 2022;399(10322):394-405.',
    year: 2022,
    journal: 'The Lancet',
    type: 'journal'
  },
  {
    id: 'r8',
    citation: 'Palmer SC, Tendal B, Mustafa RA, et al. Sodium-glucose cotransporter protein-2 (SGLT-2) inhibitors and glucagon-like peptide-1 (GLP-1) receptor agonists for type 2 diabetes: systematic review and network meta-analysis of randomised controlled trials. BMJ. 2021;372:m4573.',
    year: 2021,
    journal: 'BMJ',
    type: 'meta-analysis'
  }
];
