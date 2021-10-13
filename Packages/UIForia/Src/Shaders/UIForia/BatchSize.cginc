            
#if BATCH_SIZE_SMALL

    #define BATCH_SIZE 16

#elif BATCH_SIZE_MEDIUM

    #define BATCH_SIZE 64
    
#elif BATCH_SIZE_LARGE

    #define BATCH_SIZE 128
    
#elif BATCH_SIZE_HUGE
    
    #define BATCH_SIZE 512

#else
    
    #define BATCH_SIZE 1024
        
#endif 