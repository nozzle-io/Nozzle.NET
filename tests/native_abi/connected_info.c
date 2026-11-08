#include <stddef.h>
#include <stdio.h>
#include <nozzle/nozzle_c.h>

typedef struct { char prefix; NozzleSenderInfoSnapshot value; } SenderAlignment;
typedef struct { char prefix; NozzleConnectedSenderSnapshot value; } ConnectedAlignment;

int main(void) {
    printf("{\n");
    printf("  \"pointer_size\": %zu,\n", sizeof(void *));
    printf("  \"sender_snapshot_size\": %zu,\n", sizeof(NozzleSenderInfoSnapshot));
    printf("  \"connected_snapshot_size\": %zu,\n", sizeof(NozzleConnectedSenderSnapshot));
    printf("  \"sender_snapshot_alignment\": %zu,\n", offsetof(SenderAlignment, value));
    printf("  \"connected_snapshot_alignment\": %zu,\n", offsetof(ConnectedAlignment, value));
#define SNAPSHOT_OFFSET(type, prefix, field) \
    printf("  \"" prefix "_" #field "\": %zu,\n", offsetof(type, field))
#define SNAPSHOT_IDENTITY(type, prefix) \
    SNAPSHOT_OFFSET(type, prefix, name); \
    SNAPSHOT_OFFSET(type, prefix, application_name); \
    SNAPSHOT_OFFSET(type, prefix, id); \
    SNAPSHOT_OFFSET(type, prefix, backend); \
    printf("  \"" prefix "_name_capacity\": %zu,\n", sizeof(((type *)0)->name)); \
    printf("  \"" prefix "_application_name_capacity\": %zu,\n", sizeof(((type *)0)->application_name)); \
    printf("  \"" prefix "_id_capacity\": %zu,\n", sizeof(((type *)0)->id))
    SNAPSHOT_IDENTITY(NozzleSenderInfoSnapshot, "sender_snapshot");
    SNAPSHOT_IDENTITY(NozzleConnectedSenderSnapshot, "connected_snapshot");
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", connected);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", width);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", height);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", format);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", semantic_format);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", estimated_fps);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", frame_counter);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", last_update_time_ns);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", native_format_kind);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", native_format_value);
    SNAPSHOT_OFFSET(NozzleConnectedSenderSnapshot, "connected_snapshot", native_format_modifier);
    printf("  \"connected_snapshot_native_format_modifier_size\": %zu\n",
           sizeof(((NozzleConnectedSenderSnapshot *)0)->native_format_modifier));
#undef SNAPSHOT_IDENTITY
#undef SNAPSHOT_OFFSET
    printf("}\n");
    return ferror(stdout) ? 1 : 0;
}
