export type Json =
  | string
  | number
  | boolean
  | null
  | { [key: string]: Json | undefined }
  | Json[]

export type Database = {
  public: {
    Tables: {
      slideenhancer_pdtehd_analyses: {
        Row: {
          analysis_type: string
          created_at: string
          id: number
          presentation_title: string
          results: Json
          user_email: string
        }
        Insert: {
          analysis_type: string
          created_at?: string
          id?: number
          presentation_title: string
          results: Json
          user_email: string
        }
        Update: {
          analysis_type?: string
          created_at?: string
          id?: number
          presentation_title?: string
          results?: Json
          user_email?: string
        }
        Relationships: []
      }
      slideenhancer_pdtehd_enhancement_jobs: {
        Row: {
          ai_service_id: string
          created_at: string | null
          enhancement_options: Json | null
          id: string
          presentation_id: string | null
          status: string | null
          updated_at: string | null
          user_email: string
        }
        Insert: {
          ai_service_id: string
          created_at?: string | null
          enhancement_options?: Json | null
          id?: string
          presentation_id?: string | null
          status?: string | null
          updated_at?: string | null
          user_email: string
        }
        Update: {
          ai_service_id?: string
          created_at?: string | null
          enhancement_options?: Json | null
          id?: string
          presentation_id?: string | null
          status?: string | null
          updated_at?: string | null
          user_email?: string
        }
        Relationships: [
          {
            foreignKeyName: "slideenhancer_pdtehd_enhancement_jobs_presentation_id_fkey"
            columns: ["presentation_id"]
            isOneToOne: false
            referencedRelation: "slideenhancer_pdtehd_presentations"
            referencedColumns: ["id"]
          },
        ]
      }
      slideenhancer_pdtehd_presentations: {
        Row: {
          created_at: string | null
          id: string
          original_file_name: string
          project_id: string | null
          slide_count: number
          status: string | null
          storage_key: string
          updated_at: string | null
        }
        Insert: {
          created_at?: string | null
          id?: string
          original_file_name: string
          project_id?: string | null
          slide_count?: number
          status?: string | null
          storage_key: string
          updated_at?: string | null
        }
        Update: {
          created_at?: string | null
          id?: string
          original_file_name?: string
          project_id?: string | null
          slide_count?: number
          status?: string | null
          storage_key?: string
          updated_at?: string | null
        }
        Relationships: [
          {
            foreignKeyName: "slideenhancer_pdtehd_presentations_project_id_fkey"
            columns: ["project_id"]
            isOneToOne: false
            referencedRelation: "slideenhancer_pdtehd_projects"
            referencedColumns: ["id"]
          },
        ]
      }
      slideenhancer_pdtehd_profiles: {
        Row: {
          created_at: string | null
          first_name: string | null
          id: string
          is_premium: boolean | null
          last_login: string | null
          last_name: string | null
          roles: string[] | null
          updated_at: string | null
        }
        Insert: {
          created_at?: string | null
          first_name?: string | null
          id: string
          is_premium?: boolean | null
          last_login?: string | null
          last_name?: string | null
          roles?: string[] | null
          updated_at?: string | null
        }
        Update: {
          created_at?: string | null
          first_name?: string | null
          id?: string
          is_premium?: boolean | null
          last_login?: string | null
          last_name?: string | null
          roles?: string[] | null
          updated_at?: string | null
        }
        Relationships: []
      }
      slideenhancer_pdtehd_projects: {
        Row: {
          created_at: string | null
          description: string | null
          id: string
          name: string
          status: string | null
          updated_at: string | null
          user_email: string
        }
        Insert: {
          created_at?: string | null
          description?: string | null
          id?: string
          name: string
          status?: string | null
          updated_at?: string | null
          user_email: string
        }
        Update: {
          created_at?: string | null
          description?: string | null
          id?: string
          name?: string
          status?: string | null
          updated_at?: string | null
          user_email?: string
        }
        Relationships: []
      }
      slideenhancer_pdtehd_references: {
        Row: {
          created_at: string
          id: number
          query: string
          reference_data: Json
          user_email: string
        }
        Insert: {
          created_at?: string
          id?: number
          query: string
          reference_data: Json
          user_email: string
        }
        Update: {
          created_at?: string
          id?: number
          query?: string
          reference_data?: Json
          user_email?: string
        }
        Relationships: []
      }
      slideenhancer_pdtehd_slides: {
        Row: {
          created_at: string | null
          enhanced_content: string | null
          id: string
          is_approved: boolean | null
          is_modified: boolean | null
          original_content: string | null
          presentation_id: string | null
          slide_number: number
          updated_at: string | null
        }
        Insert: {
          created_at?: string | null
          enhanced_content?: string | null
          id?: string
          is_approved?: boolean | null
          is_modified?: boolean | null
          original_content?: string | null
          presentation_id?: string | null
          slide_number: number
          updated_at?: string | null
        }
        Update: {
          created_at?: string | null
          enhanced_content?: string | null
          id?: string
          is_approved?: boolean | null
          is_modified?: boolean | null
          original_content?: string | null
          presentation_id?: string | null
          slide_number?: number
          updated_at?: string | null
        }
        Relationships: [
          {
            foreignKeyName: "slideenhancer_pdtehd_slides_presentation_id_fkey"
            columns: ["presentation_id"]
            isOneToOne: false
            referencedRelation: "slideenhancer_pdtehd_presentations"
            referencedColumns: ["id"]
          },
        ]
      }
    }
    Views: {
      [_ in never]: never
    }
    Functions: {
      [_ in never]: never
    }
    Enums: {
      [_ in never]: never
    }
    CompositeTypes: {
      [_ in never]: never
    }
  }
}

type DefaultSchema = Database[Extract<keyof Database, "public">]

export type Tables<
  DefaultSchemaTableNameOrOptions extends
    | keyof (DefaultSchema["Tables"] & DefaultSchema["Views"])
    | { schema: keyof Database },
  TableName extends DefaultSchemaTableNameOrOptions extends {
    schema: keyof Database
  }
    ? keyof (Database[DefaultSchemaTableNameOrOptions["schema"]]["Tables"] &
        Database[DefaultSchemaTableNameOrOptions["schema"]]["Views"])
    : never = never,
> = DefaultSchemaTableNameOrOptions extends { schema: keyof Database }
  ? (Database[DefaultSchemaTableNameOrOptions["schema"]]["Tables"] &
      Database[DefaultSchemaTableNameOrOptions["schema"]]["Views"])[TableName] extends {
      Row: infer R
    }
    ? R
    : never
  : DefaultSchemaTableNameOrOptions extends keyof (DefaultSchema["Tables"] &
        DefaultSchema["Views"])
    ? (DefaultSchema["Tables"] &
        DefaultSchema["Views"])[DefaultSchemaTableNameOrOptions] extends {
        Row: infer R
      }
      ? R
      : never
    : never

export type TablesInsert<
  DefaultSchemaTableNameOrOptions extends
    | keyof DefaultSchema["Tables"]
    | { schema: keyof Database },
  TableName extends DefaultSchemaTableNameOrOptions extends {
    schema: keyof Database
  }
    ? keyof Database[DefaultSchemaTableNameOrOptions["schema"]]["Tables"]
    : never = never,
> = DefaultSchemaTableNameOrOptions extends { schema: keyof Database }
  ? Database[DefaultSchemaTableNameOrOptions["schema"]]["Tables"][TableName] extends {
      Insert: infer I
    }
    ? I
    : never
  : DefaultSchemaTableNameOrOptions extends keyof DefaultSchema["Tables"]
    ? DefaultSchema["Tables"][DefaultSchemaTableNameOrOptions] extends {
        Insert: infer I
      }
      ? I
      : never
    : never

export type TablesUpdate<
  DefaultSchemaTableNameOrOptions extends
    | keyof DefaultSchema["Tables"]
    | { schema: keyof Database },
  TableName extends DefaultSchemaTableNameOrOptions extends {
    schema: keyof Database
  }
    ? keyof Database[DefaultSchemaTableNameOrOptions["schema"]]["Tables"]
    : never = never,
> = DefaultSchemaTableNameOrOptions extends { schema: keyof Database }
  ? Database[DefaultSchemaTableNameOrOptions["schema"]]["Tables"][TableName] extends {
      Update: infer U
    }
    ? U
    : never
  : DefaultSchemaTableNameOrOptions extends keyof DefaultSchema["Tables"]
    ? DefaultSchema["Tables"][DefaultSchemaTableNameOrOptions] extends {
        Update: infer U
      }
      ? U
      : never
    : never

export type Enums<
  DefaultSchemaEnumNameOrOptions extends
    | keyof DefaultSchema["Enums"]
    | { schema: keyof Database },
  EnumName extends DefaultSchemaEnumNameOrOptions extends {
    schema: keyof Database
  }
    ? keyof Database[DefaultSchemaEnumNameOrOptions["schema"]]["Enums"]
    : never = never,
> = DefaultSchemaEnumNameOrOptions extends { schema: keyof Database }
  ? Database[DefaultSchemaEnumNameOrOptions["schema"]]["Enums"][EnumName]
  : DefaultSchemaEnumNameOrOptions extends keyof DefaultSchema["Enums"]
    ? DefaultSchema["Enums"][DefaultSchemaEnumNameOrOptions]
    : never

export type CompositeTypes<
  PublicCompositeTypeNameOrOptions extends
    | keyof DefaultSchema["CompositeTypes"]
    | { schema: keyof Database },
  CompositeTypeName extends PublicCompositeTypeNameOrOptions extends {
    schema: keyof Database
  }
    ? keyof Database[PublicCompositeTypeNameOrOptions["schema"]]["CompositeTypes"]
    : never = never,
> = PublicCompositeTypeNameOrOptions extends { schema: keyof Database }
  ? Database[PublicCompositeTypeNameOrOptions["schema"]]["CompositeTypes"][CompositeTypeName]
  : PublicCompositeTypeNameOrOptions extends keyof DefaultSchema["CompositeTypes"]
    ? DefaultSchema["CompositeTypes"][PublicCompositeTypeNameOrOptions]
    : never

export const Constants = {
  public: {
    Enums: {},
  },
} as const
