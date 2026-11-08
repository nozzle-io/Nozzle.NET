#include <stddef.h>
#include <stdio.h>
#include <nozzle/nozzle_c.h>

int main(void) {
    printf("{\n");
    printf("  \"pointer_size\": %zu,\n", sizeof(void *));
    printf("  \"size\": %zu,\n", sizeof(NozzleConnectedSenderInfo));
#define OFFSET(field) printf("  \"" #field "\": %zu,\n", offsetof(NozzleConnectedSenderInfo, field))
    OFFSET(name);
    OFFSET(application_name);
    OFFSET(id);
    OFFSET(backend);
    OFFSET(width);
    OFFSET(height);
    OFFSET(format);
    OFFSET(semantic_format);
    OFFSET(estimated_fps);
    OFFSET(frame_counter);
    OFFSET(last_update_time_ns);
    OFFSET(native_format_kind);
    OFFSET(native_format_value);
    OFFSET(native_format_modifier);
#undef OFFSET
    printf("  \"native_format_modifier_size\": %zu\n",
           sizeof(((NozzleConnectedSenderInfo *)0)->native_format_modifier));
    printf("}\n");
    return ferror(stdout) ? 1 : 0;
}
